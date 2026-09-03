# Analytics service — python-сборка через uv, мультистейдж.
FROM python:3.12-slim AS base

# uv положим системно по официальной инструкции
COPY --from=ghcr.io/astral-sh/uv:latest /uv /uvx /bin/

WORKDIR /app

# сначала одни зависимости — кэш слоя
COPY pyproject.toml ./
COPY uv.lock ./
RUN --mount=type=cache,target=/root/.cache/uv \
    uv sync --frozen --no-dev --no-install-project

COPY . .

# пакет app копируем и делаем install без dev-зависимостей
RUN --mount=type=cache,target=/root/.cache/uv \
    uv sync --frozen --no-dev

EXPOSE 8000
CMD ["uv", "run", "uvicorn", "app.main:app", "--host", "0.0.0.0", "--port", "8000"]
