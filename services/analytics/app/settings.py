"""Конфиг analytics-сервиса. Всё тащится из env-переменных."""

from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    model_config = SettingsConfigDict(env_prefix="", extra="ignore")

    # PostgreSQL — аналитика лежит в отдельной схеме analytics
    pg_host: str = "db"
    pg_port: int = 5432
    pg_db: str = "onlinecinema_db"
    pg_user: str = "cinema_user"
    pg_password: str = "change_me"
    pg_schema: str = "analytics"

    # RabbitMQ
    rabbit_host: str = "rabbitmq"
    rabbit_port: int = 5672
    rabbit_user: str = "guest"
    rabbit_password: str = "guest"
    rabbit_exchange: str = "cinema.events"
    rabbit_queue: str = "analytics.queue"

    consumer_enabled: bool = True
    # кол-во воркеров-консьюмеров в потоке
    poll_interval_seconds: float = 1.0

    @property
    def dsn(self) -> str:
        """Строка к основной БД. Каталог (public) читаем, аналитику пишем в analytics."""
        return (
            f"postgresql://{self.pg_user}:{self.pg_password}"
            f"@{self.pg_host}:{self.pg_port}/{self.pg_db}"
        )


settings = Settings()
