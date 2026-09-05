"""Деятельность пользователей: персональный и общий фид."""

from sqlalchemy import text

from .recommender import _engine
from .settings import settings

SCHEMA = settings.pg_schema


def feed_for_user(user_id: int | None, limit: int = 30) -> list[dict]:
    """Лента действий с названиями из каталога (без голых #id на фронте)."""
    # Биндим параметр дважды нельзя (psycopg ругается на тип при $1 IS NULL).
    # Проще развести два кейса, чем хитро кастить через ::int.
    base = """
        SELECT e.actor, e.kind, e.ref_type, e.ref_id, e.note, e.happened_at,
               COALESCE(m."Title", s."Title") AS ref_title
        FROM "{schema}".user_events e
        LEFT JOIN "Movies" m ON m."Id" = e.ref_id AND e.ref_type = 'movie'
        LEFT JOIN "Series" s ON s."Id" = e.ref_id AND e.ref_type = 'series'
        {where}
        ORDER BY e.happened_at DESC
        LIMIT :limit
    """
    if user_id is not None:
        q = text(base.format(schema=SCHEMA, where="WHERE e.user_id = :uid"))
        params = {"uid": user_id, "limit": limit}
    else:
        q = text(base.format(schema=SCHEMA, where=""))
        params = {"limit": limit}

    with _engine.connect() as c:
        rows = c.execute(q, params).mappings().all()
    return [dict(r) for r in rows]
