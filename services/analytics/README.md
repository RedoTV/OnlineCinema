# Analytics (Python / FastAPI / uv)

Отдельный сервис вне `src/`: слушает события каталога из RabbitMQ,
копит агрегаты в схеме `analytics` той же PostgreSQL и отдаёт фронту
тренды, топы, ленту активности и рекомендации (SVD + жанры).

Почему вне `src/`: это другой язык и рантайм (.NET там не живёт) —
`services/analytics` собирается своим Dockerfile через `uv`, а
`src/` остаётся чисто .NET + frontend.

## Связь с бэкендом

- Бэкенд публикует события в durable topic exchange `cinema.events`
  (routing keys `movie.watched`, `episode.watched`, `movie.rated`,
  `series.rated`, `user.status_changed`, `comment.created`,
  `user.registered`) — см. `src/OnlineCinema.Backend/Events/`.
- Консьюмер (`app/events.py`) объявляет тот же exchange/очередь
  идемпотентно, поэтому порядок старта не важен. Если брокер недоступен,
  бэкенд продолжает работать без событий (деградация, не падение).
- Таблицы каталога (`Movies`, `Series`, `Ratings`, …) читаются только
  на чтение; свои агрегаты сервис пишет в схему `analytics`
  (`events_raw`, `content_stats`, `user_events`).
- Оценки принимаются в обоих форматах: новом `{contentType, contentId}`
  и старом `{movieId}/{seriesId}` — см. `normalize_event` и
  `tests/test_events.py`. Тип `episode` в агрегатах не используется:
  эпизоды отображаются на родительский сериал через `Seasons`.
- Фронт ходит сюда через `analyticsApi`: в dev напрямую на
  `http://localhost:8000`, в проде через nginx-префикс `/analytics/`.

## Локальный запуск

```bash
cd services/analytics
uv run uvicorn app.main:app --host 0.0.0.0 --port 8000
```

Нужны доступные PostgreSQL и RabbitMQ (env см. `app/settings.py`:
`PG_HOST/PG_DB/PG_USER/PG_PASSWORD`, `RABBIT_HOST/RABBIT_USER/...`).
В compose всё уже прокинуто — отдельно поднимать не надо.

## Тесты и линт

```bash
cd services/analytics
uv run pytest -q
uv run ruff check app tests
```

## Эндпоинты

- `GET /health` — живость (используется healthcheck'ом в compose).
- `GET /stats/overview` — счётчики (события, фильмы, сериалы, юзеры).
- `GET /stats/trending?window=24h|7d|30d` — тренды с названиями из каталога.
- `GET /stats/top-rated?k=8` — живой топ по `Ratings`.
- `GET /stats/genres` — разбивка каталога по жанрам.
- `GET /recommendations/{user_id}?k=10` — гибрид SVD + жанры, пусто не бывает.
- `GET /recommendations/{user_id}/because?item_id=...` — «потому что смотрел X».
- `GET /activity?user_id=...&limit=...` — лента действий.
