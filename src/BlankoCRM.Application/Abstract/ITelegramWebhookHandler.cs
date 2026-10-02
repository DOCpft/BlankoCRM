using BlankoCRM.Application.DTOs.Telegram;

namespace BlankoCRM.Application.Abstract;

/// <summary>
/// Обработчик входящих обновлений от Telegram (webhook).
/// </summary>
public interface ITelegramWebhookHandler
{
    /// <summary>
    /// Обрабатывает входящее обновление Telegram.
    /// </summary>
    Task HandleUpdateAsync(TelegramUpdate update, CancellationToken ct = default);
}
