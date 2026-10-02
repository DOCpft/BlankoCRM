# BlankoCRM — Техническая документация

> Полное и самодостаточное описание системы на текущий момент.
> Документ описывает: назначение, архитектуру, модель данных, бизнес-логику, все API-эндпоинты, мультитенантность, валидацию, фоновые процессы и Telegram-уведомления.

---

## Содержание

1. [Общее описание](#1-общее-описание)
2. [Архитектура и слои](#2-архитектура-и-слои)
3. [Модель данных](#3-модель-данных)
4. [Мультитенантность](#4-мультитенантность)
5. [Аутентификация и авторизация](#5-аутентификация-и-авторизация)
6. [Бизнес-логика по модулям](#6-бизнес-логика-по-модулям)
7. [API Endpoints (полный справочник)](#7-api-endpoints-полный-справочник)
8. [Валидация и обработка ошибок](#8-валидация-и-обработка-ошибок)
9. [Unit of Work и транзакции](#9-unit-of-work-и-транзакции)
10. [Фоновые сервисы](#10-фоновые-сервисы)
11. [Telegram-уведомления и напоминания](#11-telegram-уведомления-и-напоминания)
12. [Конфигурация](#12-конфигурация)
13. [Миграции базы данных](#13-миграции-базы-данных)
14. [Запуск проекта](#14-запуск-проекта)
15. [Ограничения MVP и известные упрощения](#15-ограничения-mvp-и-известные-упрощения)

---

## 1. Общее описание

**BlankoCRM** — многотенантная CRM для мастеров и салонов красоты. Представляет собой бэкенд (REST API), который хранит базу клиентов, прайс-лист услуг, расписание (свободные/занятые слоты) и записи на приём, а также принимает публичные заявки от клиентов через VK Mini App и отправляет мастеру Telegram-уведомления.

### Целевая аудитория
- Самозанятые мастера (одиночные исполнители).
- Администраторы салонов и сетей салонов (в рамках MVP — один мастер = одна организация).

### Основные возможности
- Ведение базы клиентов с поиском по имени/телефону и доп. контактами (Telegram, VK, Instagram, WhatsApp, Viber).
- Управление услугами (название, длительность, цена, описание).
- Управление расписанием: свободные и занятые слоты, массовое создание, проверка пересечений.
- Создание и управление записями со статусной моделью.
- Публичные заявки клиентов через VK Mini App (проверка подписи VK, выбор свободного слота).
- Telegram-уведомления мастеру: о новой заявке и напоминания о предстоящих записях.
- Строгая изоляция данных между организациями (мультитенантность).
- JWT-аутентификация на базе ASP.NET Core Identity.

### Технологический стек

| Технология | Версия | Назначение |
|---|---|---|
| .NET | 9.0 (`net9.0`) | Целевая платформа |
| ASP.NET Core | 9.x | Веб-слой, контроллеры, middleware |
| Entity Framework Core | 9.0.13 | ORM, миграции |
| Npgsql.EntityFrameworkCore.PostgreSQL | 9.0.4 | Провайдер PostgreSQL |
| PostgreSQL | 15 (Docker) | СУБД |
| ASP.NET Core Identity | 9.0.13 | Пользователи, роли, пароли |
| JWT (JwtBearer) | 9.0.13 | Аутентификация |
| System.IdentityModel.Tokens.Jwt | 8.4.0 | Генерация JWT |
| AutoMapper | 12.0.1 | Entity → DTO |
| FluentValidation | 12.1.1 / 11.3.1 | Валидация запросов |
| Swashbuckle.AspNetCore | 10.1.7 | Swagger / OpenAPI |
| Serilog | 10.0.0 | Логирование |
| prometheus-net | 8.2.1 | Метрики (`/metrics`) |
| Microsoft.Extensions.Http | 9.0.13 | Typed HttpClient для Telegram |

---

## 2. Архитектура и слои

Проект построен по многослойной схеме (Clean Architecture). Решение `BlankoCRM.sln` содержит 4 production-проекта и два тестовых проекта.

| Проект | Назначение | Ключевое содержимое |
|---|---|---|
| `BlankoCRM.Core` | Домен: сущности, интерфейсы репозиториев/Unit of Work, доменные исключения | `Entities/`, `Interfaces/`, `Exceptions/` |
| `BlankoCRM.Infrastructure` | EF Core (`AppDbContext`), репозитории, миграции, внешние HTTP-клиенты | `Data/`, `Repositories/`, `ExternalServices/`, `Migrations/` |
| `BlankoCRM.Application` | Сервисы (бизнес-логика), DTO, валидаторы, маппинг, абстракции | `Services/`, `Abstract/`, `DTOs/`, `Validation/` |
| `BlankoCRM.Web` | Контроллеры, middleware, фоновые сервисы, DI, конфигурация | `Controllers/`, `Middleware/`, `BackgroundServices/` |
| `BlankoCRM.UnitTests` | Unit-тесты application-сценариев с изоляцией зависимостей | `tests/BlankoCRM.UnitTests/` |
| `BlankoCRM.IntegrationTests` | Интеграционные тесты EF Core, включая изоляцию tenant-данных | `tests/BlankoCRM.IntegrationTests/` |

### Направление зависимостей
```
Web → Application → Infrastructure → Core
Web → Infrastructure
```

> **Важно.** `Infrastructure` ссылается **только** на `Core`. Поэтому интерфейсы, реализуемые в Infrastructure (репозитории, `ITelegramBotClient`), находятся в `BlankoCRM.Core.Interfaces`. Интерфейсы, реализуемые в Application-сервисах, — в `BlankoCRM.Application.Abstract`.

### Ключевые конвенции
- Репозитории **не вызывают** `SaveChangesAsync` — сохранение только через `IUnitOfWork.SaveChangesAsync()`.
- Все мутирующие сервисы используют `ownsTransaction` (открывают транзакцию, только если её ещё нет).
- Ошибки бизнес-логики — только доменные исключения.
- `CancellationToken` — последним параметром, `async/await` повсеместно.
- Сервисы — `sealed class`, DTO — `record`.

## 3. Модель данных

Все сущности с полем `OrganizationId` реализуют интерфейс `IOrganizationEntity` и подпадают под глобальный Query Filter (см. [раздел 4](#4-мультитенантность)).

### 3.1. Обзор сущностей

| Сущность | Назначение | Связь с организацией |
|---|---|---|
| `Organization` | Тенант (мастер/салон) | корневая |
| `AppUser` | Пользователь (Identity) | 1 пользователь → 1 организация |
| `AppRole` | Роль (Identity) | — |
| `MasterProfile` | Профиль мастера | 1:1 с пользователем |
| `Salon` | Салон | N салонов → 1 организация |
| `Client` | Клиент | N клиентов → 1 организация |
| `ClientContact` | Доп. контакт клиента | через клиента |
| `Service` | Услуга (прайс) | N услуг → 1 организация |
| `Appointment` | Запись на приём | N записей → 1 организация |
| `AppointmentService` | Услуга внутри записи (снимок цены/длительности) | через запись |
| `Schedule` | Слот расписания (свободный/занятый) | N слотов → 1 организация |
| `MasterReminderSetting` | Настройка напоминания мастера | на пользователя + offset |
| `AppointmentReminderLog` | Журнал отправленных напоминаний | через запись |

### 3.2. Сущности подробно

#### Organization
`Id` (PK), `Name` (100), `Slug` (100, **unique**, lowercase — используется в публичных URL), `CreatedAt`, `UpdatedAt`.

#### AppUser (наследует `IdentityUser<Guid>`)
| Поле | Тип | Описание |
|---|---|---|
| `OrganizationId` | Guid | Тенант |
| `SalonId` | Guid? | Салон |
| `Name` | string(100) | Имя |
| `VkUserId` | long? unique (фильтр) | Привязка VK Mini App |
| `TelegramChatId` | long? unique (фильтр) | Привязанный Telegram-чат |
| `TelegramNotificationsEnabled` | bool (default true) | Включены ли уведомления |
| `TelegramLinkCode` | string? | Одноразовый код привязки |
| `TelegramLinkCodeExpiresAt` | DateTime? | Срок действия кода |
| `TimeZone` | string(64) (default "Europe/Moscow") | IANA-таймзона |

#### MasterProfile
`Id`, `OrganizationId`, `UserId` (1:1 → AppUser), `SalonId`, `Specialization`, `HireDate`, `BirthDate`.

#### Salon
`Id`, `OrganizationId`, `Name` (150), `Address`, `Phone`, `CreatedAt`, `UpdatedAt`.

#### Client
`Id`, `OrganizationId`, `SalonId`, `Name` (200), `Phone` (20, **уникален в организации**), `BirthDate`, `Notes`, `CreatedAt`, `UpdatedAt`. Навигация: `Appointments`, `Contacts`.

#### ClientContact
`Id`, `OrganizationId`, `ClientId`, `Type` (`ContactType`), `Value` (200), `CreatedAt`, `UpdatedAt`.
Уникальность: `(ClientId, Type)` — один контакт данного типа; `(OrganizationId, Type, Value)` — значение уникально в организации.

#### ContactType (enum)
`Telegram = 0`, `Vk = 1`, `Instagram = 2`, `WhatsApp = 3`, `Viber = 4`, `MAX = 5`.

#### Service
`Id`, `OrganizationId`, `SalonId`, `Name` (100, **уникально в организации**), `DurationMinutes`, `Price` (decimal 18,2), `Description`, `CreatedAt`, `UpdatedAt`.

#### Appointment
| Поле | Тип | Описание |
|---|---|---|
| `Id` | Guid | PK |
| `OrganizationId` | Guid | Тенант |
| `SalonId` | Guid? | Салон |
| `ClientId` | Guid | Клиент |
| `StartTime` / `EndTime` | DateTime | Интервал (check `EndTime >= StartTime`) |
| `Status` | `AppointmentStatus` | Статус |
| `Notes` | string? | Заметки |
| `ReminderSent` | bool | (зарезервировано) |
| `ExpiresAt` | DateTime? | Срок Pending-заявки (24 ч) |
| `CreatedAt` / `UpdatedAt` | DateTime | Служебные |

Вычисляемые поля `TotalPrice` / `TotalDuration` считаются в памяти по загруженным `AppointmentServices`.

#### AppointmentStatus (enum)
`Scheduled = 0`, `Completed = 1`, `Cancelled = 2`, `NoShow = 3`, `Archived = 4`, `Pending = 5`, `Rejected = 6`, `Withdrawn = 7`.

#### AppointmentService
`Id`, `OrganizationId`, `SalonId`, `AppointmentId`, `ServiceId`, `Price`, `DurationMinutes`, `ServiceName` (100), `Quantity` (default 1). Хранит «снимок» услуги на момент записи.

#### Schedule
`Id`, `OrganizationId`, `SalonId`, `AppointmentId` (Guid?, **unique**, nullable), `StartTime`, `EndTime`, `CreatedAt`, `UpdatedAt`.
Check: `EndTime > StartTime`. Свободный слот имеет `AppointmentId = null`.

#### MasterReminderSetting
`Id`, `OrganizationId`, `AppUserId`, `OffsetHours` (1/3/6/12/24), `Enabled`. Уникальность `(AppUserId, OffsetHours)`. Cascade-удаление при удалении пользователя.

#### AppointmentReminderLog
`Id`, `OrganizationId`, `AppointmentId`, `OffsetHours`, `SentAt`. Уникальность `(AppointmentId, OffsetHours)`. Cascade-удаление при удалении записи.

## 4. Мультитенантность

### Принцип
Каждая сущность, реализующая `IOrganizationEntity`, имеет `OrganizationId`. `AppDbContext` в `OnModelCreating` строит **глобальный Query Filter** для всех таких сущностей:

```
e => CurrentOrganizationId.HasValue && e.OrganizationId == CurrentOrganizationId.Value
```

`CurrentOrganizationId` берётся из `IOrganizationContext` (реализация `OrganizationContext` на `AsyncLocal`, scoped на запрос).

> **Важная особенность реализации:** когда контекст организации **не установлен** (`CurrentOrganizationId == null`), фильтр вычисляется как `false`, т.е. такие запросы возвращают **ноль строк**. Поэтому анонимные/фоновые сценарии, которым нужны данные всех организаций, обязаны использовать `.IgnoreQueryFilters()`.

### Как устанавливается контекст организации

1. **Авторизованные запросы (JWT).** В `Program.cs` inline-middleware читает claim `organizationId` из токена и вызывает `organizationContext.SetCurrentOrganizationId(...)`. Для неавторизованных запросов контекст сбрасывается (`Clear()`).
2. **Публичные запросы (`/api/public/{slug}/...`).** `PublicOrganizationMiddleware` находит организацию по `slug` из маршрута и устанавливает её в контекст (JWT здесь не используется).
3. **Фоновые сервисы.** Контекст не установлен → используются репозиторные методы с `IgnoreQueryFilters()` (например, `GetScheduledInWindowAsync`, `CancelExpiredPendingBatchAsync`, методы `MasterReminderSettingRepository` / `AppointmentReminderLogRepository`).

### Изоляция
- `AppUser` **не** реализует `IOrganizationEntity`, поэтому на него глобальный фильтр не действует (поиск пользователя по `OrganizationId` выполняется явно).
- Публичные endpoint'ы ищут организацию только по `slug`; `OrganizationId`/`VkUserId` из тела запроса не принимаются (безопасность).

---

## 5. Аутентификация и авторизация

Используется ASP.NET Core Identity (`AddIdentityCore<AppUser>().AddRoles<AppRole>()`).

### Способы входа
| Метод | Описание |
|---|---|
| `POST /api/auth/register` | Регистрация по email+паролю. Создаёт `Organization` (со `slug`) и `AppUser`, затем **сидирует 5 настроек напоминаний** (offsets 1,3,6,12,24; включены 6,3,1). |
| `POST /api/auth/login` | Вход по email+паролю, возвращает JWT. |
| `POST /api/auth/vk-mini-app` | Вход через VK Mini App. Проверяет подпись VK; при первом входе регистрирует пользователя (UserName `vk_{id}`, без email) и организацию. |

### JWT-токен
- Выдаётся на **7 дней**, алгоритм `HS256`.
- Claims: `sub` и `nameidentifier` (ID пользователя), `name`, `email`, **`organizationId`**, `jti`, роли, опционально `salonId`.

### Авторизация
- Контроллеры, доступные только мастеру, помечены `[Authorize]`: `ClientsController`, `ServicesController`, `AppointmentsController`, `ScheduleController`, `ProfileController`.
- `AuthController`, `PublicAppointmentsController`, `TelegramWebhookController` — `[AllowAnonymous]`.

## 6. Бизнес-логика по модулям

### 6.1. Клиенты (`ClientService`)

- **Получение/поиск**: `GetAllAsync(search)` — поиск по имени или телефону (подстрока), сортировка по имени.
- **Создание**: имя обязательно; телефон нормализуется (`PhoneNumberNormalizer`); контакты создаются по одному на тип (типы не должны повторяться).
- **Обновление**: частичное; контакты синхронизируются по типу (новые добавляются, отсутствующие удаляются, изменённые обновляются).
- **Удаление**: запрещено, если у клиента есть записи (409).

Нормализация телефона: оставляет только цифры; допустимы `+ ( ) - . /` и пробелы; 11-значный с `8` → `7...`; 10-значный → добавляет `7`; длина 1–20 цифр.

### 6.2. Услуги (`ServicesService`)

- CRUD. Название уникально в организации.
- **Удаление** запрещено, если услуга используется в записях (`AppointmentServices` не пуст) → 409.
- Длительность 1–1440 мин, цена 0–999999.99.

### 6.3. Расписание (`ScheduleService`)

Слоты бывают **свободные** (`AppointmentId = null`) и **занятые** (привязаны к записи).

- **Добавить свободный слот** (`AddFreeSlotAsync`): проверка `EndTime > StartTime`, проверка пересечения с другими свободными слотами.
- **Добавить занятый слот** (`AddOccupiedSlotAsync`): создаёт запись через `AppointmentService.CreateWithScheduleAsync`.
- **Массовое создание свободных слотов** (`AddSlotsAsync`): проверка пересечений внутри пакета и с БД.
- **Получение по периоду** (`GetScheduleByPeriodAsync`).
- **Обновление слота**: можно двигать время и/или обновлять связанную запись; у свободного слота нельзя задавать данные записи.
- **Удаление**: по ID или массово (по списку ID).

### 6.4. Записи (`AppointmentService`)

#### Создание (`CreateWithScheduleAsync`)
1. Проверяет клиента и салон.
2. Загружает услуги, создаёт снимок `AppointmentService` (цена/длительность/название).
3. Считает суммарную длительность и `EndTime`.
4. Проверяет пересечение с существующими записями (`Scheduled`/`Completed`/`NoShow`) → 409 при конфликте.
5. Создаёт `Appointment` (статус `Scheduled`) + связанный `Schedule`, удаляет пересекающиеся свободные слоты.

#### Статусная модель
```
        ┌─── Rejected (мастер отклонил)
Pending ┤
        └─── Scheduled (мастер подтвердил → создаётся слот в расписании)
               ├── Completed (услуга оказана)
               ├── NoShow (клиент не пришёл)
               └── Cancelled (отмена, слот освобождается)
Pending ── Withdrawn (клиент отозвал заявку)
Pending ── Cancelled (клиент отменил заявку)
```

| Операция | Переход | Примечание |
|---|---|---|
| `ConfirmAsync` | Pending → Scheduled | Проверяет: статус Pending, не истёк срок, нет пересечения, нет уже существующего слота. Создаёт `Schedule`, удаляет свободные слоты в интервале. |
| `RejectAsync` | Pending → Rejected | Только из Pending. |
| `CancelAsync` | Pending/Scheduled → Cancelled | Для Scheduled освобождает слот (`AppointmentId = null`). |
| `CompleteAsync` | Scheduled → Completed | Только из Scheduled. |
| `MarkNoShowAsync` | Scheduled → NoShow | Только из Scheduled. |
| `WithdrawAsync` (публичный) | Pending → Withdrawn | Только собственные заявки клиента. |

- **Редактирование** (`UpdateAsync`): запрещено для Pending. Можно менять клиента, салон, услуги, время, заметки; при смене времени — проверка пересечения.
- **Удаление** (`DeleteAsync`): запрещено для Pending; освобождает связанный слот.
- Просроченные Pending-заявки автоматически переводятся в `Cancelled` фоновым сервисом.

### 6.5. Публичные заявки (`PublicAppointmentService`, VK Mini App)

Эндпоинты анонимные, маршрут `/api/public/{organizationSlug}/appointments`. Организация определяется по `slug`; подпись VK проверяется (`VkMiniAppAuthenticator`).

- **Список свободных слотов** (`GetFreeSlotsAsync`): период до 31 дня, лимит до 500.
- **Мои записи** (`GetMyAppointmentsAsync`): активные (`Pending` + `Scheduled`) записи клиента по его VK ID.
- **Создание заявки** (`CreatePendingAsync`):
  1. Проверяет подпись VK, rate limit.
  2. Валидирует имя, услуги, время (не в прошлом).
  3. Загружает услуги организации; считает длительность и `EndTime`.
  4. Внутри транзакции (`CreatePendingAttemptAsync`): блокирует выбранный свободный слот, проверяет, что заявка помещается в слот; находит/создаёт клиента (`PublicClientResolver`); проверяет лимит активных заявок (макс. **5**) и дубликат на то же время; создаёт `Appointment` в статусе `Pending` с `ExpiresAt = now + 24ч`.
  5. После коммита отправляет Telegram-уведомление мастеру.
- **Отзыв** (`WithdrawAsync`): только собственные Pending-заявки → `Withdrawn`.
- **Отмена** (`CancelAsync`): Pending → `Cancelled` (заявка); Scheduled → `Cancelled` + освобождение слота.

### 6.6. Разрешение клиента (`PublicClientResolver`)

При создании публичной заявки клиент ищется/создаётся по VK ID и телефону:
- Найден по VK и по телефону с разными карточками → 409.
- Найден по VK: при необходимости дополняет телефон.
- Найден только по телефону: добавляет контакт VK.
- Не найден: создаёт новую карточку с контактом VK.

### 6.7. Telegram-уведомления

См. [раздел 11](#11-telegram-уведомления-и-напоминания).

## 7. API Endpoints (полный справочник)

Базовый формат ошибок — JSON `{ "error": "..." }`. См. [раздел 8](#8-валидация-и-обработка-ошибок).

### 7.1. Аутентификация — `/api/auth` (анонимно)

| Метод | Путь | Тело запроса | Ответ |
|---|---|---|---|
| POST | `/api/auth/register` | `{ name?, email, password, organizationName? }` | 201 |
| POST | `/api/auth/login` | `{ email, password }` | 200 `{ token, name, email }` |
| POST | `/api/auth/vk-mini-app` | `{ vkParams: {…sign, vk_user_id…} }` | 200 `{ token, name, email }` |

### 7.2. Клиенты — `/api/clients` ([Authorize])

| Метод | Путь | Тело/query | Ответ |
|---|---|---|---|
| GET | `/api/clients/all` | `?search=` | 200 `[ClientDto]` |
| GET | `/api/clients/{id}` | — | 200 `ClientDto` |
| POST | `/api/clients/create` | `CreateClientRequest` | 201 `ClientDto` |
| PUT | `/api/clients/update/{id}` | `UpdateClientRequest` | 204 |
| DELETE | `/api/clients/delete/{id}` | — | 204 |

`ClientDto`: `{ id, name, phone, contacts, birthDate, notes, salonId }`.
`CreateClientRequest`: `{ name, phone?, contacts?, birthDate?, notes?, salonId? }`; контакт = `{ type, value }`.

### 7.3. Услуги — `/api/services` ([Authorize])

| Метод | Путь | Тело | Ответ |
|---|---|---|---|
| GET | `/api/services` | — | 200 `[ServiceDto]` |
| GET | `/api/services/{id}` | — | 200 `ServiceDto` |
| POST | `/api/services` | `CreateServiceRequest` | 201 `ServiceDto` |
| PUT | `/api/services/{id}` | `UpdateServiceRequest` | 204 |
| DELETE | `/api/services/{id}` | — | 204 |

`CreateServiceRequest`: `{ name, durationMinutes, price, description?, salonId? }`.

### 7.4. Записи — `/api/appointments` ([Authorize])

| Метод | Путь | Тело/query | Ответ |
|---|---|---|---|
| GET | `/api/appointments/{id}` | — | 200 `AppointmentDto` |
| GET | `/api/appointments/all` | — | 200 `[AppointmentDto]` |
| GET | `/api/appointments/calendar` | `?date=` | 200 `[AppointmentDto]` |
| GET | `/api/appointments/pending` | — | 200 `[AppointmentDto]` (активные Pending) |
| POST | `/api/appointments` | `CreateAppointmentRequest` | 201 `AppointmentDto` |
| PUT | `/api/appointments/{id}` | `UpdateAppointmentRequest` | 204 |
| DELETE | `/api/appointments/{id}` | — | 204 |
| PATCH | `/api/appointments/{id}/cancel` | — | 204 |
| POST | `/api/appointments/{id}/confirm` | — | 204 |
| POST | `/api/appointments/{id}/reject` | — | 204 |
| POST | `/api/appointments/{id}/complete` | — | 204 |
| POST | `/api/appointments/{id}/no-show` | — | 204 |

`CreateAppointmentRequest`: `{ clientId, services: [ServiceInput], startTime, notes?, salonId? }`; `ServiceInput` = `{ serviceId, durationMinutes?, price?, quantity? }`.
`AppointmentDto`: `{ id, clientId, clientName, services, startTime, endTime, status, notes, totalPrice, totalDurationMinutes }`.

### 7.5. Расписание — `/api/schedule` ([Authorize])

| Метод | Путь | Тело/query | Ответ |
|---|---|---|---|
| GET | `/api/schedule/{id}` | — | 200 `ScheduleDto` |
| GET | `/api/schedule/get/period` | `?dtFrom=&dtTo=` | 200 `[ScheduleDto]` |
| POST | `/api/schedule/free` | `CreateFreeSlotRequest` | 201 `ScheduleDto` |
| POST | `/api/schedule/occupied` | `CreateOccupiedSlotRequest` | 201 `ScheduleDto` |
| POST | `/api/schedule/free/bulk` | `[CreateFreeSlotRequest]` | 200 `[ScheduleDto]` |
| PUT | `/api/schedule/{id}` | `UpdateSlotRequest` | 204 |
| DELETE | `/api/schedule/{id}` | — | 200 `bool` |
| DELETE | `/api/schedule/delete/bulk` | `[Guid]` | 200 `int` (количество) |

`CreateFreeSlotRequest`: `{ startTime, endTime, salonId? }`.
`CreateOccupiedSlotRequest`: `{ startTime, appointmentData: { clientId, services, notes?, salonId? }, endTime? }`.

### 7.6. Публичные заявки — `/api/public/{organizationSlug}/appointments` (анонимно, VK)

| Метод | Путь | Тело/query | Ответ |
|---|---|---|---|
| GET | `.../appointments/slots` | `?from=&to=&limit=` (limit default 100) | 200 `[PublicFreeSlotDto]` |
| POST | `.../appointments/mine` | `{ vkParams }` | 200 `[AppointmentDto]` |
| POST | `.../appointments` | `CreatePublicAppointmentRequest` | 201 `AppointmentDto` |
| POST | `.../appointments/{id}/withdraw` | `{ vkParams }` | 204 |
| POST | `.../appointments/{id}/cancel` | `{ vkParams }` | 204 |

`PublicFreeSlotDto`: `{ id, startTime, endTime }`.
`CreatePublicAppointmentRequest`: `{ freeSlotId, vkParams, name, phone?, services: [PublicServiceInput], startTime, notes? }`; `PublicServiceInput` = `{ serviceId, quantity? }`.

> Rate limiting: ASP.NET RateLimiter по IP (`create` 10/мин, `withdraw` 20/мин, `read` 60/мин) + отдельный in-memory `PublicVkRateLimiter` по подтверждённому VK ID (`Create` 5/мин, `Withdraw` 10/мин).

### 7.7. Профиль мастера — `/api/profile` ([Authorize])

| Метод | Путь | Тело | Ответ |
|---|---|---|---|
| POST | `/api/profile/telegram/link` | — | 200 `TelegramLinkResponse` |
| DELETE | `/api/profile/telegram/link` | — | 204 |
| GET | `/api/profile/telegram/status` | — | 200 `TelegramLinkStatus` |
| GET | `/api/profile/reminder-settings` | — | 200 `[ReminderSettingDto]` |
| PUT | `/api/profile/reminder-settings` | `UpdateReminderSettingsRequest` | 204 |
| PATCH | `/api/profile/timezone` | `{ timeZone }` | 204 |

`TelegramLinkResponse`: `{ url, botUsername }`.
`TelegramLinkStatus`: `{ linked, notificationsEnabled }`.
`ReminderSettingDto`: `{ offsetHours, enabled }`; `UpdateReminderSettingsRequest`: `{ settings: [{offsetHours, enabled}] }`.

### 7.8. Webhook Telegram — `/api/telegram` (анонимно)

| Метод | Путь | Заголовки | Ответ |
|---|---|---|---|
| POST | `/api/telegram/webhook` | `X-Telegram-Bot-Api-Secret-Token` | 200 (или 401 при неверном секрете) |

Тело — стандартное `Update` от Telegram Bot API (`message.text` начинается с `/start`). Всегда возвращает 200, чтобы Telegram не ретраил.

---

## 8. Валидация и обработка ошибок

### Доменные исключения (`BlankoCRM.Core.Exceptions`)

| Исключение | HTTP | Использование |
|---|---|---|
| `ValidationException` | 400 | Нарушение бизнес-валидации |
| `NotFoundException` | 404 | Сущность не найдена |
| `ConflictException` | 409 | Бизнес-конфликт (пересечение, недопустимый переход статуса и т.п.) |
| `UnauthorizedException` | 401 | Неверные учётные данные |
| `TooManyRequestsException` | 429 | Превышен лимит запросов |
| `TelegramForbiddenException` | 403 | Telegram-бот заблокирован пользователем |

Все наследуют `DomainException` (содержит `StatusCode`).

### Middleware `ExceptionHandlingMiddleware`
Перехватывает исключения и возвращает JSON `{ "error": "..." }`:
1. `DomainException` → статус из `StatusCode`.
2. `UnauthorizedAccessException` → 401.
3. `DbUpdateException`/`PostgresException` → маппинг по `SqlState` (`UniqueViolation` → 409, `ExclusionViolation` → 409, `ForeignKeyViolation` → 400, `CheckViolation` → 400, иначе 500).
4. Прочие → 500.

### FluentValidation
Валидаторы в `Application/Validation/RequestValidators.cs`, регистрируются через `AddValidatorsFromAssembly` + `AddFluentValidationAutoValidation`. Основные правила: лимиты длин, диапазоны цен/длительности, уникальность типов контактов, валидность VK-параметров, допустимые offsets напоминаний (1/3/6/12/24).

## 9. Unit of Work и транзакции

- Интерфейс `IUnitOfWork` (`Core/Interfaces/IUnitOfWork.cs`) и реализация `EfUnitOfWork` (`Infrastructure/Data/EfUnitOfWork.cs`).
- Репозитории только регистрируют изменения; `SaveChangesAsync` вызывается только из `IUnitOfWork`.
- Все мутирующие сервисы следуют паттерну:

```csharp
var ownsTransaction = !_unitOfWork.HasActiveTransaction;
await using var transaction = ownsTransaction
    ? await _unitOfWork.BeginTransactionAsync(ct)
    : null;
try
{
    // ... изменения ...
    await _unitOfWork.SaveChangesAsync(ct);
    if (ownsTransaction) await transaction!.CommitAsync(ct);
}
catch
{
    if (ownsTransaction && _unitOfWork.HasActiveTransaction) await transaction!.RollbackAsync(ct);
    throw;
}
```

- Для защиты от гонок используются пессимистичные блокировки через сырой SQL: `LockForUpdateAsync` (записи), `LockFreeSlotForUpdateAsync` (слоты), `LockForPendingLimitAsync` (клиент для лимита заявок).
- HTTP-запросы к Telegram выполняются **вне** транзакций БД.

---

## 10. Фоновые сервисы

| Сервис | Периодичность | Назначение |
|---|---|---|
| `PendingAppointmentExpirationWorker` | 1 минута | Переводит просроченные Pending-заявки в `Cancelled` (пакетами по 100). |
| `AppointmentReminderWorker` | 5 минут | Рассылает напоминания о предстоящих Scheduled-записях. |

Оба создают scope на каждой итерации, ловят и логируют ошибки, не «роняют» процесс.

---

## 11. Telegram-уведомления и напоминания

### Компоненты

| Компонент | Слой | Назначение |
|---|---|---|
| `ITelegramBotClient` | Core.Interfaces | Низкоуровневый HTTP-клиент Telegram Bot API |
| `TelegramBotClient` | Infrastructure.ExternalServices | Typed HttpClient (Base URL `https://api.telegram.org`) |
| `ITelegramNotificationService` / `TelegramNotificationService` | Application | Формирование и отправка уведомлений |
| `ITelegramLinkService` / `TelegramLinkService` | Application | Привязка/отвязка Telegram-аккаунта |
| `ITelegramWebhookHandler` / `TelegramWebhookHandler` | Application | Обработка входящего `/start` |
| `IReminderSettingService` / `ReminderSettingService` | Application | Настройки напоминаний |
| `IReminderDispatchService` / `ReminderDispatchService` | Application | Рассылка напоминаний |
| `ITelegramBotClient`-реализация | Infrastructure | HTTP к Telegram |

### Сценарии
1. **Привязка Telegram**: мастер запрашивает `POST /api/profile/telegram/link` → получает URL `https://t.me/{bot}?start={code}` (код действует 10 минут). При нажатии Start в боте webhook находит пользователя по коду и сохраняет `TelegramChatId`.
2. **Новая заявка**: после создания Pending-заявки мастеру отправляется сообщение (имя клиента, услуги, время в его таймзоне, заметки).
3. **Напоминание**: worker за 5 минут до `StartTime + offset` находит Scheduled-записи и отправляет напоминание (если offset включён в настройках и ещё не отправлялось для этой пары).
4. **Заблокированный бот**: при 403 от Telegram (`TelegramForbiddenException`) `TelegramChatId` сбрасывается, ошибка логируется, основной флоу не ломается.

### Обработка ошибок Telegram (`TelegramBotClient`)
- 403 → `TelegramForbiddenException` (вызывающий код сбрасывает chatId).
- 429 → читает `retry_after`, логирует, не ретраит.
- 400/401 и прочие → логирует, не пробрасывает.
- Сообщения — plain text (без MarkdownV2).

### Формат времени
`DateTimeFormatter.FormatInTimeZone(utc, ianaTimeZone)` конвертирует UTC в таймзону мастера (`TimeZoneInfo`), формат `dd.MM.yyyy HH:mm`.

### Настройки напоминаний
- При регистрации мастера создаются 5 записей `MasterReminderSetting` с offsets `[1, 3, 6, 12, 24]`; включены по умолчанию `6, 3, 1`.
- Мастер управляет включением через `PUT /api/profile/reminder-settings` (допустимые offsets 1/3/6/12/24).
- Повторные напоминания исключены уникальным индексом `(AppointmentId, OffsetHours)`.

---

## 12. Конфигурация

`appsettings.json`:

```json
{
  "ConnectionStrings": { "DefaultConnection": "Host=localhost;Port=5433;Database=blankocrm;Username=blanko;Password=blanko_pass" },
  "Jwt": { "Key": "..." },
  "VkApp": { "Id": "...", "Secret": "..." },
  "Telegram": { "BotToken": "", "BotUsername": "blanko_crm_bot", "WebhookSecret": "" },
  "Serilog": { ... },
  "LogSinks": { "Seq": { "Uri": "http://localhost:5341" } }
}
```

- Секреты в dev — через `dotnet user-secrets` (не логировать `BotToken`, `WebhookSecret`, `TelegramChatId`).
- Если `Telegram:BotToken` пуст — приложение логирует Warning и продолжает работать (уведомления отключены).

## 13. Миграции базы данных

Миграции лежат в `src/BlankoCRM.Infrastructure/Migrations` (EF Core, Npgsql). Применяются автоматически при старте (`dbContext.Database.MigrateAsync()` в `Program.cs`).

Создать новую миграцию:
```bash
dotnet ef migrations add <Name> \
  --project src/BlankoCRM.Infrastructure/BlankoCRM.Infrastructure.csproj \
  --startup-project src/BlankoCRM.Web/BlankoCRM.Web.csproj
```

Применить:
```bash
dotnet ef database update \
  --project src/BlankoCRM.Infrastructure/BlankoCRM.Infrastructure.csproj \
  --startup-project src/BlankoCRM.Web/BlankoCRM.Web.csproj
```

---

## 14. Запуск проекта

1. Поднять PostgreSQL (порт 5433, база `blankocrm`, пользователь `blanko`).
2. Настроить `appsettings.json` (или user-secrets) — строка подключения, `Jwt:Key`, при необходимости `Telegram`.
3. Сборка и запуск:
```bash
cd d:\kirill
dotnet build BlankoCRM.sln
dotnet run --project src/BlankoCRM.Web/BlankoCRM.Web.csproj
```
4. Swagger UI доступен на `/swagger`, метрики Prometheus — `/metrics`.

### Тесты

Unit-тесты бизнес-сервисов находятся в `tests/BlankoCRM.UnitTests` и запускаются без внешней базы данных:
```bash
dotnet test tests/BlankoCRM.UnitTests/BlankoCRM.UnitTests.csproj
```

Тесты EF Core находятся в `tests/BlankoCRM.IntegrationTests`. Они проверяют query filters на InMemory provider, в частности недоступность записей другой организации, и не требуют запуска Docker или PostgreSQL:
```bash
dotnet test tests/BlankoCRM.IntegrationTests/BlankoCRM.IntegrationTests.csproj
```
InMemory provider не эмулирует поведение PostgreSQL и не проверяет database constraints, включая unique, exclusion и check constraints. Для проверки этих ограничений потребуется отдельный набор тестов на PostgreSQL.

---

## 15. Ограничения MVP и известные упрощения

- Клиентские уведомления (VK Notify, Telegram для клиента) **не реализованы** — отдельная задача.
- Telegram-сообщения — только plain text (MarkdownV2 осознанно не используется из-за сложности экранирования).
- Нет retry при ошибках Telegram (429/400/401 — только логирование).
- Нет очереди сообщений (RabbitMQ/Hangfire), кэширования `AppUser`, таблицы шаблонов сообщений.
- Rate limiting исходящих в Telegram отсутствует.
- Рассылка напоминаний — 5 простых SQL-запросов (по одному на offset), без единого большого запроса.
- Привязка «один мастер = одна организация» (поиск мастера по `OrganizationId` берёт первого пользователя).
- Unit-тесты application-сценариев находятся в `tests/BlankoCRM.UnitTests`; интеграционные EF Core проверки — в `tests/BlankoCRM.IntegrationTests`. Автоматизированный E2E-набор не входит в текущий скоуп.
- В `ScheduleService`/`AppointmentService` часть логов и текстов ошибок — на русском, часть — на английском (исторически).







