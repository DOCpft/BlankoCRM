using System.Net;
using System.Text;
using System.Text.Json;
using BlankoCRM.Core.Exceptions;
using BlankoCRM.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BlankoCRM.Infrastructure.ExternalServices;

/// <summary>
/// Низкоуровневый HTTP-клиент Telegram Bot API.
/// </summary>
public sealed class TelegramBotClient : ITelegramBotClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TelegramBotClient> _logger;

    public TelegramBotClient(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<TelegramBotClient> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    private string? BotToken => _configuration["Telegram:BotToken"];

    public async Task SendMessageAsync(long chatId, string text, CancellationToken ct = default)
    {
        var token = BotToken;
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("Telegram BotToken не настроен. Сообщение не отправлено.");
            return;
        }

        var payload = JsonSerializer.Serialize(new { chat_id = chatId, text = text });
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/bot{token}/sendMessage")
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Не удалось отправить сообщение Telegram.");
            return;
        }

        using (response)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            switch ((int)response.StatusCode)
            {
                case 403:
                    _logger.LogWarning("Telegram вернул 403 Forbidden. Бот заблокирован пользователем.");
                    throw new TelegramForbiddenException("Telegram-бот заблокирован пользователем.");
                case 429:
                    var retryAfter = await ReadRetryAfterAsync(response, ct);
                    _logger.LogWarning("Превышен лимит Telegram (429). retry_after={RetryAfter}", retryAfter);
                    return;
                case 400:
                case 401:
                    _logger.LogError("Telegram отклонил сообщение со статусом {StatusCode}.", (int)response.StatusCode);
                    return;
                default:
                    _logger.LogError("Telegram вернул неожиданный статус {StatusCode}.", (int)response.StatusCode);
                    return;
            }
        }
    }

    public async Task SetWebhookAsync(string url, string secretToken, CancellationToken ct = default)
    {
        var token = BotToken;
        if (string.IsNullOrWhiteSpace(token))
        {
            _logger.LogWarning("Telegram BotToken не настроен. Webhook не установлен.");
            return;
        }

        var payload = JsonSerializer.Serialize(new { url, secret_token = secretToken });
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/bot{token}/setWebhook")
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };

        try
        {
            using var response = await _httpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Telegram setWebhook завершился со статусом {StatusCode}.", (int)response.StatusCode);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Не удалось установить webhook Telegram.");
        }
    }

    private async Task<int?> ReadRetryAfterAsync(HttpResponseMessage response, CancellationToken ct)
    {
        try
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            using var document = JsonDocument.Parse(body);
            if (document.RootElement.TryGetProperty("parameters", out var parameters)
                && parameters.TryGetProperty("retry_after", out var retryAfter)
                && retryAfter.TryGetInt32(out var seconds))
            {
                return seconds;
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Не удалось прочитать retry_after из ответа Telegram.");
        }

        return null;
    }
}
