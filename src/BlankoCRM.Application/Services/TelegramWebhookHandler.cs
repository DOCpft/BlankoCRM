using BlankoCRM.Application.Abstract;
using BlankoCRM.Application.DTOs.Telegram;
using BlankoCRM.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace BlankoCRM.Application.Services;

/// <summary>
/// Обрабатывает входящие обновления Telegram (только команду /start).
/// Ошибки не пробрасывает, чтобы Telegram получал 200.
/// </summary>
public sealed class TelegramWebhookHandler : ITelegramWebhookHandler
{
    private readonly ITelegramLinkService _telegramLinkService;
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly ILogger<TelegramWebhookHandler> _logger;

    public TelegramWebhookHandler(
        ITelegramLinkService telegramLinkService,
        ITelegramBotClient telegramBotClient,
        ILogger<TelegramWebhookHandler> logger)
    {
        _telegramLinkService = telegramLinkService;
        _telegramBotClient = telegramBotClient;
        _logger = logger;
    }

    public async Task HandleUpdateAsync(TelegramUpdate update, CancellationToken ct = default)
    {
        try
        {
            var message = update?.Message;
            if (message?.Text is null
                || !message.Text.StartsWith("/start", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (message.Chat?.Id is null)
            {
                return;
            }

            var chatId = message.Chat.Id.Value;
            var code = ExtractStartCode(message.Text);

            if (string.IsNullOrWhiteSpace(code))
            {
                await _telegramBotClient.SendMessageAsync(
                    chatId,
                    "Привет! Чтобы привязать уведомления, откройте личный кабинет в CRM.",
                    ct);
                return;
            }

            await _telegramLinkService.HandleStartCommandAsync(code, chatId, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Не удалось обработать обновление Telegram.");
        }
    }

    private static string? ExtractStartCode(string text)
    {
        var spaceIndex = text.IndexOf(' ');
        if (spaceIndex < 0)
        {
            return null;
        }

        var code = text[(spaceIndex + 1)..].Trim();
        return string.IsNullOrWhiteSpace(code) ? null : code;
    }
}
