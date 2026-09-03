# Тестирование и демо-данные

Дипломный проект руками проверяется так, без .http файлов.

## Демо-данные

Бэкенд сам наполняет каталог при старте, если в `.env` стоит:

```bash
SEED_DEMO_DATA=true
```

Генерит жанры, актёров, 30 фильмов и 6 сериалов (сезоны + эпизоды).
Логика в `src/OnlineCinema.Backend/Services/DemoSeeder.cs`. Работает
только на пустой базе — повторный запуск уже не дублирует.

```bash
cd backend
docker compose up -d
# чтобы пересоздать чистую базу:
docker compose down -v
```

## Смоук-тест API

Ставит весь критичный путь: рега -> логин -> каталог -> сериалы ->
оценка/статус -> комменты/лайки -> статистика. Отдаёт понятный ититог.

```bash
# все контейнеры подняты, каталок отдельным флагом
node scripts/smoke-test.mjs http://localhost:5000/api
```

Нужен Node 18+ (глобальный fetch, никаких зависимостей).
Код: `scripts/smoke-test.mjs`.

## Postman

`docs/postman/OnlineCinema.postman_collection.json` — импортируй в Postman.
Задай переменные `base_url` и `token`. Токен сам подставляется скриптом
на коллекции после Register/Login (автовиждет в environment).
