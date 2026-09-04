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

## Проверка

- Все 6 контейнеров Up: PostgreSQL и MinIO healthy, RabbitMQ healthy, backend, frontend/nginx, analytics.
- .NET Release tests: **3/3**; analytics `uv run pytest`: **3/3**; frontend production build: passed.
- API smoke: **16/16** (auth, movies, series structure, rating/status, comments/replies/likes, stats, streaming contract).
- Admin login и API: dashboard/comments/ratings 200; movie create/update/delete 201/200/204; comment hide/approve 200.
- Frontend `/`, `/admin`; nginx API and analytics proxy: 200.
- Analytics health, overview, genres, activity, recommendations: 200.
- Media proxy: poster redirects to same-origin `/media/...`; byte range returns 206.
- RabbitMQ: durable topic exchange `cinema.events`; published smoke events consumed, analytics `processed_events` grows.
- Fresh logs contain no `PoolClosed`, password-auth, upstream DNS, missing-exchange, unhandled or SigV4/403 errors.

## Осознанные ограничения

Видео и пользовательские постеры не коммитятся в Git. Seeder создаёт автономные SVG-постеры; видео загружается администратором, чтобы репозиторий не раздувался. Backend не буферизует видео: браузер получает подписанный MinIO URL через nginx Range proxy.
