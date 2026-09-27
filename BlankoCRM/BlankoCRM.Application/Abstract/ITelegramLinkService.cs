using BlankoCRM.Application.DTOs;

namespace BlankoCRM.Application.Abstract;

/// <summary>
/// Сервис управления привязкой Telegram-аккаунта мастера.
/// </summary>
public interface ITelegramLinkService
{
    /// <summary>
    /// Генерирует одноразовый код и ссылку для привязки Telegram-аккаунта.
    /// </summary>
    Task<TelegramLinkResponse> GenerateLinkAsync(CancellationToken ct = default);

    /// <summary>
    /// Отвязывает Telegram-аккаунт мастера.
    /// </summary>
    Task UnlinkAsync(CancellationToken ct = default);

    /// <summary>
    /// Возвращает текущий статус привязки Telegram-аккаунта.
    /// </summary>
    Task<TelegramLinkStatus> GetStatusAsync(CancellationToken ct = default);

    /// <summary>
    /// Обрабатывает команду /start от Telegram и привязывает чат по коду.
    /// </summary>
    Task HandleStartCommandAsync(string code, long chatId, CancellationToken ct = default);
}
