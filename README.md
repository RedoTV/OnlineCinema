# OnlineCinema

Дипломный проект — агрегатор и стриминговый сервис фильмов и сериалов.

Полиглот-архитектура: ASP.NET Core (.NET 10) бэкенд, React (Vite) фронт,
PostgreSQL, MinIO (S3-совместимое хранилище), в v3.0 — RabbitMQ + Python
(FastAPI/uv) аналитический сервис.

## Возможности

- Каталог фильмов и сериалов (сезоны/эпизоды) с поиском
- JWT-авторизация: регистрация/вход, роли User/Admin
- Оценки 1–10, статусы «в планах/просмотрено/избранное»
- Комментарии с ответами, лайками и модерацией
- Стриминг видео через presigned-URL из MinIO (по HTTP Range, без
  буферизации на бэкенде) с автоматическим сохранением прогресса
- Статистика по жанрам и топам (v2.0 — простая, из основной БД;
  в v3.0 уезжает в analytics-сервис)
- Плеер на Plyr (в v3.0 добавлю качество/субтитры)

## Запуск

Нужен Docker с Docker Compose.

```bash
cp .env.example .env          # поставь свои секреты
# если хочешь сразу наполнить каталог демо-контентом:
# в .env поставь SEED_DEMO_DATA=true

docker compose up -d
```

Открой http://localhost:3000 . Бэкенд-API на http://localhost:5000/swagger,
MinIO-консоль http://localhost:9001 (minioadmin/minioadmin123).

Если каталог пуст, сначала поставь `SEED_DEMO_DATA=true` в `.env`,
перезапусти backend и дай ему минуту на сидинг.

## Проверка руками (без .http)

```bash
# наполнить базу демо-контентом
# .env: SEED_DEMO_DATA=true, затем docker compose up -d

# пройти критичный путь через API
node scripts/smoke-test.mjs http://localhost:5000/api

# postman коллекция лежит в docs/postman/
```

Подробнее: `scripts/README.md`

## Тесты и CI

Юнит-тесты backend (xUnit):

```bash
dotnet test tests/OnlineCinema.Backend.Tests
```

CI в GitHub Actions: сборка backend/frontend → линт → тесты →
docker compose up → смоук. На теги `v*` выпускается релиз с образами
в GHCR и чейнджлогом (git-cliff).

## Структура

```
src/
  OnlineCinema.Backend/   # ASP.NET Core 10 API
  OnlineCinema.Frontend/  # React + Vite + Tailwind
tests/
  OnlineCinema.Backend.Tests/
scripts/                  # смоук-тест, README по тестированию
docs/
  postman/                # коллекция Postman
```

## Ветки

`develop` — интеграция фич, `main` — релизы. Каждая фича живёт в
`feature/*` и мержится через PR. Релизные ветки `release/*`, теги `v*`.

## Известные косяки

- Плеер иногда не сразу подхватывает позицию при перемотке на новом
  эпизоде (гонка между запросом прогресса и загрузкой видео).
- Seeder работает только на пустой базе — если надо чисто, дропай
  volume (`docker compose down -v`).
- `DisableRequestSizeLimit` на загрузке видео остался — для прода надо
  вводить лимит, но на дипломе сойдёт.
- Рекомендации и по-настоящему живая статистика — в v3.0, тут пока
  агрегаты из общей БД.

---

Made for the diploma defense. v2.0 = профессиональный фундамент,
v3.0 = event-driven аналитика и рекомендации.
