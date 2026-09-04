import json
import logging
import time

import pika

from .db import get_conn
from .settings import settings

log = logging.getLogger(__name__)


def _upsert_content_stats(conn, content_type, content_id, views=0, watch_seconds=0, grade=None):
    """Агрегируем в часовую корзину 'сиюминутной' статистики контента."""
    import datetime

    from zoneinfo import ZoneInfo

    bucket = datetime.datetime.now(ZoneInfo("UTC")).replace(minute=0, second=0, microsecond=0)
    with conn.cursor() as cur:
        cur.execute(
            f"""
            INSERT INTO "{settings.pg_schema}".content_stats
                (bucket_hour, content_type, content_id, views, watch_seconds, rating_sum, rating_count)
            VALUES (%s, %s, %s, %s, %s, %s, %s)
            ON CONFLICT (bucket_hour, content_type, content_id)
            DO UPDATE SET
                views = "{settings.pg_schema}".content_stats.views + EXCLUDED.views,
                watch_seconds = "{settings.pg_schema}".content_stats.watch_seconds + EXCLUDED.watch_seconds,
                rating_sum = "{settings.pg_schema}".content_stats.rating_sum + EXCLUDED.rating_sum,
                rating_count = "{settings.pg_schema}".content_stats.rating_count + EXCLUDED.rating_count
            """,
            (bucket, content_type, content_id, views, watch_seconds, grade or 0, 1 if grade else 0),
        )


def _record_user_event(conn, user_id, username, kind, ref_type, ref_id, note=""):
    with conn.cursor() as cur:
        cur.execute(
            f"""
            INSERT INTO "{settings.pg_schema}".user_events (user_id, actor, kind, ref_type, ref_id, note)
            VALUES (%s, %s, %s, %s, %s, %s)
            """,
            (user_id, username, kind, ref_type, ref_id, note),
        )


def _handle(routing_key: str, payload: dict) -> None:
    """Разбирает одно событие и пишет в аналитику."""
    with get_conn() as conn:
        # сырое событие - чтобы можно было пересчитать агрегаты потом
        with conn.cursor() as cur:
            cur.execute(
                f'INSERT INTO "{settings.pg_schema}".events_raw (routing_key, payload) VALUES (%s, %s)',
                (routing_key, json.dumps(payload)),
            )

        uid = payload.get("userId") or payload.get("user_id")
        uname = payload.get("username") or payload.get("actor") or ""

        if routing_key in ("movie.watched", "episode.watched"):
            content_type = "movie" if "movieId" in payload else "episode"
            # series episodes сводим к серии для трендов — упрощённо к эпизоду, но
            # картинку тренда даёт. Для кино смотрим отдельной записью.
            cid = payload.get("movieId") or payload.get("episodeId")
            secs = int(payload.get("watchSeconds") or 0)
            _upsert_content_stats(conn, "movie", cid or 0, views=1, watch_seconds=secs)
            _record_user_event(conn, uid, uname, "watched", content_type, cid)

        elif routing_key in ("movie.rated", "series.rated"):
            content_type = "movie" if "movieId" in payload else "series"
            cid = payload.get("movieId") or payload.get("seriesId")
            grade = payload.get("grade") or payload.get("rating")
            _upsert_content_stats(conn, content_type, cid or 0, grade=grade)
            _record_user_event(conn, uid, uname, "rated", content_type, cid, note=f"оценил в {grade}/10")

        elif routing_key == "user.status_changed":
            state = payload.get("state", "")
            cid = payload.get("movieId")
            if state == "Watched":
                _upsert_content_stats(conn, "movie", cid or 0, views=1)
                _record_user_event(conn, uid, uname, "watched", "movie", cid)
            elif state == "Favorite":
                _record_user_event(conn, uid, uname, "favorite", "movie", cid)

        elif routing_key == "comment.created":
            ctype = "movie" if payload.get("movieId") else "series"
            cid = payload.get("movieId") or payload.get("seriesId")
            _record_user_event(conn, uid, uname, "comment", ctype, cid)

        elif routing_key == "user.registered":
            _record_user_event(conn, uid, uname, "registered", None, None)

        conn.commit()


def _run_once(channel):
    method, properties, body = channel.basic_get(settings.rabbit_queue, auto_ack=False)
    if not method:
        return False
    try:
        payload = json.loads(body or b"{}")
        _handle(method.routing_key, payload)
        channel.basic_ack(method.delivery_tag)
        return True
    except Exception:
        log.exception("Кривое событие %s, скипаю", method.routing_key)
        # nack без requeue чтобы не крутить вечно битое
        channel.basic_nack(method.delivery_tag, requeue=False)
        return True


def consumer_loop(stop_flag: list[bool]) -> None:
    """Демонический цикл: держит коннект к RabbitMQ и разгребает очередь."""
    reconnect_sleep = 5.0
    while not stop_flag[0]:
        try:
            creds = pika.PlainCredentials(settings.rabbit_user, settings.rabbit_password)
            params = pika.ConnectionParameters(
                host=settings.rabbit_host,
                port=settings.rabbit_port,
                credentials=creds,
                heartbeat=30,
            )
            conn = pika.BlockingConnection(params)
            ch = conn.channel()
            # Consumer may start before the .NET publisher. Declaring the same durable
            # topic exchange is idempotent and removes the startup race.
            ch.exchange_declare(exchange=settings.rabbit_exchange, exchange_type="topic", durable=True)
            ch.queue_declare(queue=settings.rabbit_queue, durable=True)
            ch.queue_bind(queue=settings.rabbit_queue, exchange=settings.rabbit_exchange, routing_key="#")

            log.info("Анализ подписался на %s", settings.rabbit_exchange)
            reconnect_sleep = 5.0
            while not stop_flag[0]:
                processed_any = _run_once(ch)
                if not processed_any:
                    time.sleep(settings.poll_interval_seconds)
            conn.close()
            break
        except Exception:
            if stop_flag[0]:
                break
            log.warning("Broker пока недоступен, ретрай через %ss", reconnect_sleep)
            time.sleep(reconnect_sleep)
