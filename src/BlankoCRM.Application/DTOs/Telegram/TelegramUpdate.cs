namespace BlankoCRM.Application.DTOs.Telegram;

/// <summary>
/// Входящее обновление Telegram Bot API (сокращённая модель, только message_new).
/// </summary>
public sealed record TelegramUpdate(
    long UpdateId,
    TelegramMessage? Message);

/// <summary>
/// Входящее сообщение Telegram.
/// </summary>
public sealed record TelegramMessage(
    long MessageId,
    TelegramChat? Chat,
    string? Text);

/// <summary>
/// Чат Telegram, из которого пришло сообщение.
/// </summary>
public sealed record TelegramChat(
    long? Id,
    string? Type);
