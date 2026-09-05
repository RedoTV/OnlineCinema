"""Деятельность пользователей: персональный и общий фид."""

from sqlalchemy import text

from .recommender import _engine
from .settings import settings

SCHEMA = settings.pg_schema


def feed_for_user(user_id: int | None, limit: int = 30) -> list[dict]:
    # Биндим параметр дважды нельзя (psycopg ругается на тип при $1 IS NULL).
    # Проще развести два кейса, чем хитро кастить через ::int.
    if user_id is not None:
        q = text(
            f"""
            SELECT actor, kind, ref_type, ref_id, note, happened_at
            FROM "{SCHEMA}".user_events
            WHERE user_id = :uid
            ORDER BY happened_at DESC
            LIMIT :limit
            """
        )
        params = {"uid": user_id, "limit": limit}
    else:
        q = text(
            f"""
            SELECT actor, kind, ref_type, ref_id, note, happened_at
            FROM "{SCHEMA}".user_events
            ORDER BY happened_at DESC
            LIMIT :limit
            """
        )
        params = {"limit": limit}

    with _engine.connect() as c:
        rows = c.execute(q, params).mappings().all()
    return [dict(r) for r in rows]
