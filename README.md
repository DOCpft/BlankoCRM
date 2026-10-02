# BlankoCRM

Многотенантная CRM для мастеров и салонов красоты. Backend предоставляет REST API для клиентов, услуг, расписания и записей, принимает публичные заявки из VK Mini App и отправляет уведомления через Telegram.

## Структура репозитория

- `src/BlankoCRM.Web` — ASP.NET Core API и web-инфраструктура.
- `src/BlankoCRM.Application` — сценарии приложения, DTO и валидация.
- `src/BlankoCRM.Core` — доменные сущности и интерфейсы.
- `src/BlankoCRM.Infrastructure` — EF Core, PostgreSQL, репозитории и внешние интеграции.
- `tests/BlankoCRM.UnitTests` — unit-тесты application-сервисов.
- `tests/BlankoCRM.IntegrationTests` — интеграционные проверки EF Core и изоляции данных по организациям.
- `docs/` — [техническая документация](docs/DOCUMENTATION.md), [сценарии](docs/USE_CASES.md) и [спецификация разработки](docs/AI_DEVELOPMENT_SPEC.md).
- `docker/` — Dockerfile и Compose-конфигурации.
- `infra/prometheus/` — настройки сбора метрик.

## Требования

- .NET SDK 9
- Docker и Docker Compose v2 (для контейнерного запуска)

## Запуск API локально

1. Запустите зависимости (PostgreSQL и Seq):

   ```powershell
   Copy-Item docker/.env.example docker/.env
   docker compose --env-file docker/.env -f docker/docker-compose.yml up -d postgres seq
   ```

2. Запустите API из корня репозитория:

   ```powershell
   dotnet run --project src/BlankoCRM.Web/BlankoCRM.Web.csproj
   ```

   Параметры локального запуска находятся в `src/BlankoCRM.Web/appsettings.json` и `appsettings.Development.json`. Адрес API по умолчанию — `http://localhost:5102`; Swagger доступен в Development, метрики — `/metrics`.

## Запуск полного Docker-стека

Создайте локальный файл переменных и задайте уникальные секреты (в частности для production):

```powershell
Copy-Item docker/.env.example docker/.env
```

Запуск API вместе с PostgreSQL, Seq, Prometheus и Grafana в Development:

```powershell
docker compose --env-file docker/.env -f docker/docker-compose.yml -f docker/docker-compose.dev.yml up --build -d
```

Production-профиль:

```powershell
docker compose --env-file docker/.env -f docker/docker-compose.yml -f docker/docker-compose.prod.yml up --build -d
```

Production требует заданных `POSTGRES_PASSWORD`, `JWT_KEY`, `VK_APP_ID`, `VK_APP_SECRET`, `TELEGRAM_BOT_TOKEN` и `TELEGRAM_WEBHOOK_SECRET`. Не добавляйте `docker/.env` с секретами в Git.

Сервисы доступны на портах API `5102`, PostgreSQL `5433`, Seq `5341`, Prometheus `9090` и Grafana `3000`. Для остановки стека выполните ту же команду Compose с `down`.

## Сборка и тесты

```powershell
dotnet build BlankoCRM.sln
dotnet test tests/BlankoCRM.UnitTests/BlankoCRM.UnitTests.csproj
dotnet test tests/BlankoCRM.IntegrationTests/BlankoCRM.IntegrationTests.csproj
```

Unit-тесты изолируют зависимости сервисов с помощью Moq. Integration-тесты проверяют поведение EF Core на InMemory provider и также не требуют запущенной базы данных или Docker.

## Документация

Подробное описание архитектуры, API, конфигурации и миграций находится в [docs/DOCUMENTATION.md](docs/DOCUMENTATION.md). Пользовательские сценарии перечислены в [docs/USE_CASES.md](docs/USE_CASES.md).
