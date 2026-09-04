# BUGS — v2 coursework demo

Проверено на чистых volumes командой `docker compose down -v --remove-orphans`, затем `docker compose up -d --build`.

## Исправлено

### BUG-1 — frontend image did not build
- **Симптом:** Vite 7 падал/предупреждал на Node 18.
- **Причина:** Vite требует Node >= 20.19.
- **Фикс:** build stage переведён на `node:22-alpine`, установка сделана воспроизводимой через `npm ci`.

### BUG-2 — nginx не отдавал постеры и видео из MinIO
- **Симптом:** API редиректил браузер на `http://minio:9000`; Docker DNS с хоста недоступен.
- **Причина:** presigned SigV4 URL подписан с Host `minio:9000`, простая замена на localhost даёт 403.
- **Фикс:** backend возвращает same-origin `/media/...`; nginx проксирует в MinIO с исходным Host.
- **Range:** `Range`/`If-Range` передаются, buffering выключен. Проверен ответ `206 Partial Content`.

### BUG-3 — пустые постеры после первого запуска
- **Симптом:** каталог был заполнен, но `PosterUrl` оставался NULL.
- **Фикс:** seeder кладёт маленькие автономные SVG-постеры в MinIO: 12 фильмов и все 6 сериалов. Сеть/сторонний API не нужны.

### BUG-4 — пустые оценки и комментарии
- **Симптом:** блоки рейтингов/обсуждения невозможно нормально показать сразу после запуска.
- **Фикс:** добавлены 3 demo users, 6 ratings, корневой комментарий и ответ. Повторный запуск не дублирует данные.

### BUG-5 — хрупкая JWT-конфигурация
- **Симптом:** при пропущенном секрете backend падал с неясным `Encoding.GetBytes(null)`.
- **Фикс:** явная startup validation с понятным сообщением; `.env.example` содержит полный набор ключей.

### BUG-6 — startup order frontend/MinIO
- **Симптом:** nginx мог стартовать до готовности MinIO media upstream.
- **Фикс:** frontend зависит от healthy MinIO и запущенного backend.

## Результаты проверки

- `dotnet test -c Release`: **3/3 passed**.
- `npm run build`: **passed**, React/Vite production bundle создан.
- `nginx -t`: **passed**.
- HTTP: frontend `/` 200, SPA fallback 200, `/api/Movies` через nginx 200, Swagger 200, MinIO health 200.
- Seed: 30 movies, 6 series, 31 seasons, 250 episodes, 12 actors, 8 genres, 6 ratings, 2 comments.
- Media: poster endpoint 302 на `/media/...`, media 200, Range `bytes=0-127` → 206 и корректный Content-Range.
- `node scripts/smoke-test.mjs`: **16/16 passed** (auth, catalog, series structure, ratings, statuses, comments, stats, streaming contract).
- Логи сервисов: нет `Unhandled`, `PoolClosed`, password-auth, DNS или SigV4/403 ошибок.

## Границы v2

RabbitMQ, analytics microservice и персональные рекомендации относятся к v3 и намеренно не входят в v2 Compose. В coursework demo используются базовые агрегаты из основной БД. Видео-файлы не коммитятся в Git; Range-путь проверен на объекте MinIO, а видео загружается администратором перед показом streaming UI.
