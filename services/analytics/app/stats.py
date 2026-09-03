"""Агрегаты дашборда: тренды за период, жанровая разбивка, активность.

Читаем из нашей analytics.consumer_ schema (агрегаты) и живых
оценок каталога для мгновенного топ-рейтинга.
"""

import datetime
from zoneinfo import ZoneInfo

from sqlalchemy import text

from .settings import settings
from .recommender import _engine

SCHEMA = settings.pg_schema


def trending(window_hours: int = 7 * 24) -> list[dict]:
    """Что смотрят/оценивают последние N часов."""
    since = datetime.datetime.now(ZoneInfo("UTC")) - datetime.timedelta(hours=window_hours)
    q = text(
        f"""
        SELECT content_type, content_id,
               SUM(views) AS views,
               SUM(watch_seconds) AS watch_seconds
        FROM "{SCHEMA}".content_stats
        WHERE bucket_hour >= :since
        GROUP BY content_type, content_id
        ORDER BY (SUM(views) + SUM(watch_seconds)/1800.0) DESC
        LIMIT 12
        """
    )
    with _engine.connect() as c:
        rows = c.execute(q, {"since": since}).mappings().all()
    return [dict(r) for r in rows]


def genre_mix() -> list[dict]:
    """Жанровая разбивка каталога (реальный count из Movies)."""
    q = text(
        """
        SELECT g."Name" AS genre, COUNT(DISTINCT mg."MoviesId") AS cnt
        FROM "Genres" g
        LEFT JOIN "MovieGenres" mg ON mg."GenresId" = g."Id"
        GROUP BY g."Name"
        ORDER BY cnt DESC
        """
    )
    with _engine.connect() as c:
        rows = c.execute(q).mappings().all()
    return [dict(r) for r in rows]


def live_top_rated(k: int = 8) -> list[dict]:
    q = text(
        """
        SELECT m."Id", m."Title",
               AVG(r."RatingValue") AS avg_rating,
               COUNT(r."Id") AS n_ratings
        FROM "Movies" m
        JOIN "Ratings" r ON r."MovieId" = m."Id"
        GROUP BY m."Id"
        ORDER BY avg_rating DESC, n_ratings DESC
        LIMIT :k
        """
    )
    with _engine.connect() as c:
        rows = c.execute(q, {"k": k}).mappings().all()
    return [dict(r) for r in rows]


def overview() -> dict:
    q = text(
        f"""
        SELECT
          (SELECT count(*) FROM "{SCHEMA}".events_raw) AS processed_events,
          (SELECT count(*) FROM "Movies") AS total_movies,
          (SELECT count(*) FROM "Series") AS total_series,
          (SELECT count(DISTINCT "UserId") FROM "Ratings") AS active_users,
          (SELECT count(*) FROM "UserMovieStatuses" WHERE "Status" = 'Watched') AS total_watches
        """
    )
    with _engine.connect() as c:
        row = c.execute(q).mappings().one()
    return dict(row)
