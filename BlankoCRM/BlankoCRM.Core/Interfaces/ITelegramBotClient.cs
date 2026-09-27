namespace BlankoCRM.Core.Interfaces;

/// <summary>
/// Низкоуровневый HTTP-клиент Telegram Bot API.
/// </summary>
public interface ITelegramBotClient
{
    /// <summary>
    /// Отправляет текстовое сообщение (plain text) в указанный чат.
    /// </summary>
    /// <param name="chatId">Идентификатор чата Telegram.</param>
    /// <param name="text">Текст сообщения.</param>
    /// <param name="ct">Токен отмены.</param>
    Task SendMessageAsync(long chatId, string text, CancellationToken ct = default);

    /// <summary>
    /// Устанавливает webhook для получения входящих обновлений.
    /// </summary>
    /// <param name="url">Публичный URL обработчика.</param>
    /// <param name="secretToken">Секретный токен для проверки запросов.</param>
    /// <param name="ct">Токен отмены.</param>
    Task SetWebhookAsync(string url, string secretToken, CancellationToken ct = default);
}
