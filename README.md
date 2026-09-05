# OnlineCinema

Дипломный проект — агрегатор и стриминговый сервис фильмов и сериалов.

Полиглот-архитектура: ASP.NET Core (.NET 10) бэкенд, React (Vite) фронт,
PostgreSQL, MinIO (S3-совместимое хранилище), RabbitMQ и Python
(FastAPI/uv) аналитический сервис. Это единая линия **v2.0**; актуальный
срез опубликован как `v2.0.3`.

## Возможности

- Каталог фильмов и сериалов (сезоны/эпизоды) с поиском
- JWT-авторизация: регистрация/вход, роли User/Admin
- Оценки 1–10, статусы «в планах/просмотрено/избранное»
- Комментарии с ответами, лайками и модерацией
- Стриминг видео через presigned-URL из MinIO (по HTTP Range, без
  буферизации на бэкенде) с автоматическим сохранением прогресса
- Статистика по жанрам и топам из основной БД и отдельный событийный
  analytics-сервис с рекомендациями
- Плеер на Plyr с сохранением позиции, скоростью, PiP и автопереходом
  к следующему эпизоду

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
services/
  analytics/            # Python/FastAPI сервис событий и рекомендаций (uv)
tests/
  OnlineCinema.Backend.Tests/
scripts/                  # смоук-тест, README по тестированию
docs/
  postman/                # коллекция Postman
```

Подробнее про аналитику: `services/analytics/README.md`.

## Ветки

`develop` — интеграция изменений, `master` — стабильная версия. Работа
ведётся через короткоживущие ветки и PR; после слияния такие ветки удаляются.
Текущая и единственная поддерживаемая линия релизов — `v2.0.x`.

## Известные косяки

- Плеер иногда не сразу подхватывает позицию при перемотке на новом
  эпизоде (гонка между запросом прогресса и загрузкой видео).
- Seeder работает только на пустой базе — если надо чисто, дропай
  volume (`docker compose down -v`).
- Для доставки больших видео nginx отключает буферизацию запроса, а
  приложение ограничивает размер видео 10 GiB.
- Для гарантированной доставки событий RabbitMQ пока нет transactional
  outbox: при недоступности брокера основная операция продолжает работать.

---

Made for the diploma defense. **v2.0** объединяет каталог, стриминг,
администрирование, событийную аналитику и рекомендации.
