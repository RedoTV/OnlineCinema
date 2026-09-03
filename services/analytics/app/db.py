"""Доступ к PostgreSQL.

Аналитика пишется в отдельную схему `analytics`, чтобы не путаться с
каталогом (.NET владеет public). Каталог (genres/actors/ratings) читаем
только на чтение — это нужно для фич-векторов рекомендаций и трендов.
"""

import psycopg
from psycopg.rows import dict_row
from psycopg_pool import ConnectionPool

from .settings import settings

_pool = ConnectionPool(settings.dsn, min_size=1, max_size=10, open=False, kwargs={"row_factory": dict_row})


def get_conn():
    return _pool.connection()


def init_schema() -> None:
    """Создаёт схему analytics и таблицы, если их нет."""
    with psycopg.connect(settings.dsn) as conn:
        with conn.cursor() as cur:
            cur.execute(f'CREATE SCHEMA IF NOT EXISTS "{settings.pg_schema}"')
            cur.execute(
                f"""
                CREATE TABLE IF NOT EXISTS "{settings.pg_schema}".events_raw (
                    id BIGSERIAL PRIMARY KEY,
                    routing_key TEXT NOT NULL,
                    payload JSONB NOT NULL,
                    created_at TIMESTAMPTZ DEFAULT now()
                )
                """
            )
            cur.execute(
                # часовая корзина «сколько смотрели/оценили» по контенту
                f"""
                CREATE TABLE IF NOT EXISTS "{settings.pg_schema}".content_stats (
                    id BIGSERIAL PRIMARY KEY,
                    bucket_hour TIMESTAMPTZ NOT NULL,
                    content_type TEXT NOT NULL,      -- movie | series
                    content_id INT NOT NULL,
                    views INT NOT NULL DEFAULT 0,
                    watch_seconds BIGINT NOT NULL DEFAULT 0,
                    rating_sum INT NOT NULL DEFAULT 0,
                    rating_count INT NOT NULL DEFAULT 0,
                    UNIQUE (bucket_hour, content_type, content_id)
                )
                """
            )
            cur.execute(
                # пользовательский профиль: последние действия для activity feed
                f"""
                CREATE TABLE IF NOT EXISTS "{settings.pg_schema}".user_events (
                    id BIGSERIAL PRIMARY KEY,
                    user_id INT NOT NULL,
                    actor TEXT,                     -- username, если знаем
                    kind TEXT NOT NULL,             -- watched/rated/status/comment
                    ref_type TEXT,                  -- movie/series
                    ref_id INT,
                    note TEXT,
                    happened_at TIMESTAMPTZ NOT NULL DEFAULT now()
                )
                """
            )
            cur.execute(
                # удобный индекс для фида
                f'CREATE INDEX IF NOT EXISTS idx_user_events_uid_time ON "{settings.pg_schema}".user_events (user_id, happened_at DESC)'
            )
        conn.commit()
