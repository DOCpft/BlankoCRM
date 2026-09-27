namespace BlankoCRM.Application.DTOs;

/// <summary>
/// Ответ со ссылкой для привязки Telegram-аккаунта.
/// </summary>
/// <param name="Url">Полная ссылка вида https://t.me/{bot}?start={code}.</param>
/// <param name="BotUsername">Имя бота без "@".</param>
public record TelegramLinkResponse(string Url, string BotUsername);

/// <summary>
/// Текущий статус привязки Telegram-аккаунта.
/// </summary>
/// <param name="Linked">Привязан ли Telegram-чат.</param>
/// <param name="NotificationsEnabled">Включены ли уведомления.</param>
public record TelegramLinkStatus(bool Linked, bool NotificationsEnabled);

/// <summary>
/// Запрос на изменение часового пояса мастера.
/// </summary>
/// <param name="TimeZone">IANA-идентификатор часового пояса (например, "Europe/Moscow").</param>
public record UpdateTimeZoneRequest(string? TimeZone);
