"""Деятельность пользователей: персональный и общий фид."""

from sqlalchemy import text

from .settings import settings
from .recommender import _engine

SCHEMA = settings.pg_schema


def feed_for_user(user_id: int | None, limit: int = 30) -> list[dict]:
    q = text(
        f"""
        SELECT actor, kind, ref_type, ref_id, note, happened_at
        FROM "{SCHEMA}".user_events
        WHERE (:uid IS NULL OR user_id = :uid)
        ORDER BY happened_at DESC
        LIMIT :limit
        """
    )
    with _engine.connect() as c:
        rows = c.execute(q, {"uid": user_id, "limit": limit}).mappings().all()
    return [dict(r) for r in rows]
