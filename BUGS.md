# BUGS — coursework demo validation

Проверено на чистых volumes: `docker compose down -v --remove-orphans`, затем `docker compose up -d --build`.

## Исправлено

1. **Frontend image не собирался:** Vite 7 запускался на Node 18. Docker build переведён на Node 22 + `npm ci`.
2. **nginx падал с `host not found in upstream analytics`:** analytics теперь штатная часть шестисервисного Compose; зависимости запуска заданы явно.
3. **Постеры и видео не открывались из браузера:** presigned URL содержал внутренний `minio:9000`; замена hostname ломала SigV4. Backend возвращает `/media/...`, nginx сохраняет подписанный Host.
4. **Range streaming:** nginx раньше не имел media location. Передаются `Range`/`If-Range`, buffering выключен. Подтверждён `206 Partial Content` и корректный Content-Range.
5. **Неполный demo seed:** добавлены оценки, комментарии, пользователи, MinIO-постеры и недостающие сериалы. Свежая БД содержит 30 фильмов, 9 сериалов, 43 сезона, 304 эпизода, 12 актёров, 8 жанров, 6 оценок и 2 комментария.
6. **Пустые рекомендации:** SVD на малой выборке мог вернуть `[]`. Добавлены content/popularity/recent-catalog fallbacks; проверено 5 результатов на свежей БД.
7. **Analytics `PoolClosed`:** psycopg pool открывается до consumer и закрывается в lifespan shutdown.
8. **Неверные EF join columns:** analytics использует `MoviesId`/`GenresId`, поэтому genre stats и recommender больше не падают.
9. **RabbitMQ DNS:** broker подключён к общей сети Compose.
10. **RabbitMQ startup race:** analytics пытался bind queue до создания `cinema.events`. Consumer теперь идемпотентно объявляет durable topic exchange сам.
11. **Activity 500 без user id:** запросы с фильтром и без фильтра разделены, ambiguous parameter устранён.
12. **JWT null crash:** обязательный секрет валидируется при старте понятным сообщением.
13. **Не было frontend Admin Panel:** добавлен защищённый `/admin` в прежней чёрно-белой стилистике: overview, CRUD фильмов/сериалов/актёров/жанров, poster upload, hide/approve/delete comments, ratings table.
14. **Не хватало admin read API:** добавлены `/api/Admin/dashboard`, `/comments`, `/ratings` под ролью Admin.
15. **Analytics терял оценки:** бэкенд шлёт `movie.rated`/`series.rated` как `{contentType, contentId}`, а консьюмер ждал `{movieId}` — в `content_stats` копилась мусорная строка `series/0`. Добавлен `normalize_event` (оба формата), эпизоды маппятся на сериал через `Seasons`, username подтягивается из `Users`.
16. **Trending без названий:** `/stats/trending` отдавал голые id, дашборд вёл сериалы на `/movie/{id}`. Тренды обогащены `LEFT JOIN` к каталогу, ссылки ведут на `/movie` или `/series` по типу.
17. **Analytics без обвязки:** нет README, нет healthcheck в compose, CI не гонял `uv`-тесты. Добавлены `services/analytics/README.md`, healthcheck `/health`, CI-job `test-analytics` (pytest + ruff).
18. **Страница `/analytics` не открывалась:** префикс `location /analytics/` в nginx перехватывал SPA-роут дашборда — nginx 301-редиректил на `http://localhost/analytics/` с потерей порта, а `/analytics/` уходило в API (404 от FastAPI). API переехало на `/api/analytics/`, страница снова отдаёт SPA 200. Заодно: лента обогащена `ref_title` из каталога (вместо голых `#id`), в рекомендациях видна причина, у дашборда появились loading/error/retry вместо тихих пустот.
19. **Тренд «👁 0 · 0с» выглядел сломанным:** оценка создавала агрегат с `views=0`, а формула тренда оценки игнорировала. `/stats/trending` теперь отдаёт `rating_count`/`avg_grade`, ранжирует с их учётом, фронт показывает `★ 9.0` и прячет нулевые метрики.
20. **Аналитика переехала в .NET, шина и Python-сервис удалены:** виджеты трендов/топов/жанров/активности и подборки по любимым жанрам теперь считаются живыми EF-запросами в `AnalyticsController`/`AnalyticsRepository` по таблицам каталога. Удалены Python `services/analytics`, RabbitMQ-шина (`Events/`, publisher, `RabbitMQ.Client`), событийная схема и связанные dockr/CI/фронт-клиенты. Стек — 4 контейнера: PostgreSQL, MinIO, backend, frontend/nginx.

## Проверка

- Все 4 контейнера Up: PostgreSQL и MinIO healthy, backend, frontend/nginx.
- .NET Release tests: **6/6**; frontend production build: passed.
- API smoke: auth, movies, series, ratings/status, comments, stats, streaming, analytics overview/trending/genres/activity/picks — 200.
- Admin login и API: dashboard/comments/ratings; movie create/update/delete.
- Frontend `/`, `/analytics`, `/admin`; nginx `/api` proxy: 200.
- Media proxy: poster redirects to same-origin `/media/...`; byte range returns 206.
- Fresh logs contain no unhandled or SigV4/403 errors.

## Осознанные ограничения

Видео и пользовательские постеры не коммитятся в Git. Seeder создаёт автономные SVG-постеры; видео загружается администратором, чтобы репозиторий не раздувался. Backend не буферизует видео: браузер получает подписанный MinIO URL через nginx Range proxy.
