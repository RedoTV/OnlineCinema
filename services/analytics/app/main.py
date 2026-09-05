"""FastAPI-точка входа analytics-сервиса OnlineCinema.

При старте создаёт аналитическую схему в PG и поднимает фоновый поток-
консьюмер RabbitMQ. Эндпоинты читают агрегаты и отдают JSON.

Запуск (из services/analytics):
    uv run uvicorn app.main:app --host 0.0.0.0 --port 8000
"""

import logging
import threading
from contextlib import asynccontextmanager

from fastapi import FastAPI, Query
from fastapi.middleware.cors import CORSMiddleware

from .db import init_schema
from .events import consumer_loop
from .settings import settings
from . import stats, activity, recommender

logging.basicConfig(level=logging.INFO)
log = logging.getLogger("analytics")

_stop = [False]


@asynccontextmanager
async def lifespan(_app: FastAPI):
    init_schema()
    log.info("Схема %s готова", settings.pg_schema)

    if settings.consumer_enabled:
        t = threading.Thread(target=consumer_loop, args=(_stop,), daemon=True, name="rabbit-consumer")
        t.start()
        log.info("Консьюмер событий запущен")

    yield

    _stop[0] = True
    from .db import close_pool
    close_pool()
    log.info("Analytics завершает работу")


app = FastAPI(title="OnlineCinema Analytics", version="0.1.0", lifespan=lifespan)

# dev-фронт ходит напрямую на :8000 во время разработки — открываем корс
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_methods=["*"],
    allow_headers=["*"],
)


@app.get("/health")
def health():
    return {"status": "ok"}


@app.get("/stats/overview")
def overview():
    return stats.overview()


@app.get("/stats/trending")
def trending(window: str = Query("7d")):
    # window: 24h | 7d | 30d
    hours = {"24h": 24, "7d": 168, "30d": 720}.get(window, 168)
    rows = stats.trending(window_hours=hours)
    # подтягиваем заголовки из каталога
    return rows


@app.get("/stats/genres")
def genres():
    return stats.genre_mix()


@app.get("/stats/top-rated")
def top_rated(k: int = Query(8, le=20)):
    return stats.live_top_rated(k)


@app.get("/recommendations/{user_id}")
def recommendations(user_id: int, k: int = Query(10, le=20)):
    return recommender.hybrid_for_user(user_id, k=k)


@app.get("/recommendations/{user_id}/because")
def because(user_id: int, item_id: int = Query(...), k: int = 8):
    # контент-лояльное подобие к конкретному фильму
    return recommender.content_recs(item_id, k)


@app.get("/activity")
def user_feed(user_id: int | None = Query(None), limit: int = Query(30)):
    return activity.feed_for_user(user_id, limit)
