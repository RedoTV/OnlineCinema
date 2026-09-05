import json
import logging
import time

import pika

from .db import get_conn
from .settings import settings

log = logging.getLogger(__name__)

# Канонические типы контента в content_stats/user_events: только movie | series.
# Эпизоды отображаем на родительский сериал (см. _series_for_episode), чтобы
# id эпизодов не смешивались с id фильмов в одном бакете.
MOVIE = "movie"
SERIES = "series"


def _as_int(value):
    try:
        if value is None:
            return None
        return int(value)
    except (TypeError, ValueError):
        return None


def normalize_event(routing_key: str, payload: dict) -> dict:
    """Сводит оба формата событий к каноническому виду (чистая функция, без БД).

    Бэкенд (.NET) сейчас шлёт оценки как {contentType, contentId}, а старые
    сообщения в очереди лежат как {movieId}/{seriesId} — принимаем оба.
    Возвращает {"user_id", "stats", "event", "episode_id"}:
      stats: {"content_type", "content_id", "views", "watch_seconds", "grade"} | None
      event: {"kind", "ref_type", "ref_id", "note"} | None
      episode_id: int | None — когда серию надо доискать в БД по эпизоду.
    """
    p = payload if isinstance(payload, dict) else {}
    uid = p.get("userId")
    if uid is None:
        uid = p.get("user_id")
    user_id = _as_int(uid)

    def _pick(*keys):
        for k in keys:
            if p.get(k) is not None:
                return p.get(k)
        return None

    movie_id = _as_int(_pick("movieId", "movie_id"))
    series_id = _as_int(_pick("seriesId", "series_id"))
    episode_id = _as_int(_pick("episodeId", "episode_id"))
    ctype_raw = str(_pick("contentType", "content_type") or "").lower()
    cid_raw = _as_int(_pick("contentId", "content_id"))
    grade = _as_int(_pick("grade", "rating"))
    secs_raw = _pick("watchSeconds", "watch_seconds")
    try:
        watch_seconds = int(secs_raw) if secs_raw is not None else 0
    except (TypeError, ValueError):
        watch_seconds = 0
    state = str(p.get("state") or "")

    # Новый формат оценок: contentType/contentId без явного movieId/seriesId.
    if movie_id is None and ctype_raw == MOVIE and cid_raw is not None:
        movie_id = cid_raw
    if series_id is None and ctype_raw == SERIES and cid_raw is not None:
        series_id = cid_raw

    if routing_key == "movie.watched":
        if movie_id is None:
            return {"user_id": user_id, "stats": None, "event": None, "episode_id": None}
        return {
            "user_id": user_id,
            "stats": {"content_type": MOVIE, "content_id": movie_id,
                      "views": 1, "watch_seconds": watch_seconds, "grade": None},
            "event": {"kind": "watched", "ref_type": MOVIE, "ref_id": movie_id, "note": ""},
            "episode_id": None,
        }

    if routing_key == "episode.watched":
        if episode_id is None:
            return {"user_id": user_id, "stats": None, "event": None, "episode_id": None}
        # content_id доищем в _handle по Seasons/Episodes; событие пока без ref.
        return {
            "user_id": user_id,
            "stats": None,
            "event": {"kind": "watched", "ref_type": SERIES, "ref_id": None, "note": ""},
            "episode_id": episode_id,
        }

    if routing_key == "movie.rated":
        if movie_id is None:
            return {"user_id": user_id, "stats": None, "event": None, "episode_id": None}
        note = f"оценил в {grade}/10" if grade is not None else ""
        return {
            "user_id": user_id,
            "stats": {"content_type": MOVIE, "content_id": movie_id,
                      "views": 0, "watch_seconds": 0, "grade": grade},
            "event": {"kind": "rated", "ref_type": MOVIE, "ref_id": movie_id, "note": note},
            "episode_id": None,
        }

    if routing_key == "series.rated":
        if series_id is None:
            return {"user_id": user_id, "stats": None, "event": None, "episode_id": None}
        note = f"оценил в {grade}/10" if grade is not None else ""
        return {
            "user_id": user_id,
            "stats": {"content_type": SERIES, "content_id": series_id,
                      "views": 0, "watch_seconds": 0, "grade": grade},
            "event": {"kind": "rated", "ref_type": SERIES, "ref_id": series_id, "note": note},
            "episode_id": None,
        }

    if routing_key == "user.status_changed":
        if state == "Watched" and movie_id is not None:
            return {
                "user_id": user_id,
                "stats": {"content_type": MOVIE, "content_id": movie_id,
                          "views": 1, "watch_seconds": 0, "grade": None},
                "event": {"kind": "watched", "ref_type": MOVIE, "ref_id": movie_id, "note": ""},
                "episode_id": None,
            }
        if state == "Favorite":
            return {
                "user_id": user_id, "stats": None,
                "event": {"kind": "favorite", "ref_type": MOVIE, "ref_id": movie_id, "note": ""},
                "episode_id": None,
            }
        if state == "Planned":
            return {
                "user_id": user_id, "stats": None,
                "event": {"kind": "planned", "ref_type": MOVIE, "ref_id": movie_id, "note": ""},
                "episode_id": None,
            }
        return {"user_id": user_id, "stats": None, "event": None, "episode_id": None}

    if routing_key == "comment.created":
        if movie_id is not None:
            ref_type, ref_id = MOVIE, movie_id
        elif series_id is not None:
            ref_type, ref_id = SERIES, series_id
        else:
            ref_type, ref_id = None, None
        return {
            "user_id": user_id, "stats": None,
            "event": {"kind": "comment", "ref_type": ref_type, "ref_id": ref_id, "note": ""},
            "episode_id": None,
        }

    if routing_key == "user.registered":
        return {
            "user_id": user_id, "stats": None,
            "event": {"kind": "registered", "ref_type": None, "ref_id": None, "note": ""},
            "episode_id": None,
        }

    # Неизвестный ключ — сырое событие всё равно сохраним, агрегатов нет.
    return {"user_id": user_id, "stats": None, "event": None, "episode_id": None}


def _lookup_username(conn, user_id: int | None) -> str:
    if user_id is None:
        return ""
    with conn.cursor() as cur:
        cur.execute('SELECT "Username" FROM "Users" WHERE "Id" = %s', (user_id,))
        row = cur.fetchone()
    if not row:
        return ""
    if isinstance(row, dict):
        return row.get("Username") or ""
    return row[0] or ""


def _series_for_episode(conn, episode_id: int | None) -> int | None:
    """Родительский сериал эпизода через Seasons (EF: Episodes.SeasonId -> Seasons.SeriesId)."""
    if episode_id is None:
        return None
    with conn.cursor() as cur:
        cur.execute(
            """
            SELECT sn."SeriesId"
            FROM "Episodes" e JOIN "Seasons" sn ON sn."Id" = e."SeasonId"
            WHERE e."Id" = %s
            """,
            (episode_id,),
        )
        row = cur.fetchone()
    if not row:
        return None
    if isinstance(row, dict):
        return row.get("SeriesId")
    return row[0]


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
    """Разбирает одно событие и пишет в аналитику (идемпотентно к формату)."""
    norm = normalize_event(routing_key, payload)
    uid = norm["user_id"]
    if uid is None:
        log.warning("Событие %s без userId, сырьё сохраню, агрегатов нет", routing_key)
    with get_conn() as conn:
        # сырое событие - чтобы можно было пересчитать агрегаты потом
        with conn.cursor() as cur:
            cur.execute(
                f'INSERT INTO "{settings.pg_schema}".events_raw (routing_key, payload) VALUES (%s, %s)',
                (routing_key, json.dumps(payload)),
            )

        uname = _lookup_username(conn, uid)

        stats = norm["stats"]
        event = norm["event"]

        if norm["episode_id"] is not None:
            sid = _series_for_episode(conn, norm["episode_id"])
            if sid is not None:
                secs = 0
                if isinstance(payload, dict):
                    try:
                        secs = int(payload.get("watchSeconds")
                                   or payload.get("watch_seconds") or 0)
                    except (TypeError, ValueError):
                        secs = 0
                stats = {"content_type": SERIES, "content_id": int(sid),
                         "views": 1, "watch_seconds": secs, "grade": None}
                if event is not None:
                    event = {**event, "ref_type": SERIES, "ref_id": int(sid)}
            else:
                log.warning("Эпизод %s не найден в каталоге, событие %s без агрегата",
                            norm["episode_id"], routing_key)

        if stats is not None and uid is not None:
            _upsert_content_stats(conn, stats["content_type"], stats["content_id"],
                                  views=stats.get("views", 0),
                                  watch_seconds=stats.get("watch_seconds", 0),
                                  grade=stats.get("grade"))
        if event is not None and uid is not None:
            _record_user_event(conn, uid, uname, event["kind"],
                               event.get("ref_type"), event.get("ref_id"),
                               note=event.get("note") or "")

        conn.commit()


def _run_once(channel):
    method, _properties, body = channel.basic_get(settings.rabbit_queue, auto_ack=False)
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
        except Exception:  # noqa: BLE001 — цикл обязан пережить любой сбой брокера
            if stop_flag[0]:
                break
            log.warning("Broker пока недоступен, ретрай через %ss", reconnect_sleep)
            time.sleep(reconnect_sleep)
