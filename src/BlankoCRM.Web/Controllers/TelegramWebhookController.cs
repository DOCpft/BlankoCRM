using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using BlankoCRM.Application.Abstract;
using BlankoCRM.Application.DTOs.Telegram;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlankoCRM.Web.Controllers;

/// <summary>
/// Webhook для входящих обновлений Telegram Bot API.
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/telegram")]
public sealed class TelegramWebhookController : ControllerBase
{
    private readonly ITelegramWebhookHandler _handler;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TelegramWebhookController> _logger;

    public TelegramWebhookController(
        ITelegramWebhookHandler handler,
        IConfiguration configuration,
        ILogger<TelegramWebhookController> logger)
    {
        _handler = handler;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Принимает обновления от Telegram. Всегда возвращает 200 (кроме неверного секрета).
    /// </summary>
    [HttpPost("webhook")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Webhook(CancellationToken ct)
    {
        if (!IsValidSecret(Request))
        {
            return Unauthorized();
        }

        try
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync(ct);

            TelegramUpdate? update = null;
            try
            {
                update = JsonSerializer.Deserialize<TelegramUpdate>(body, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (JsonException exception)
            {
                _logger.LogWarning(exception, "Не удалось разобрать тело webhook Telegram.");
            }

            if (update is not null)
            {
                await _handler.HandleUpdateAsync(update, ct);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Непредвиденная ошибка при обработке webhook Telegram.");
        }

        return Ok();
    }

    private bool IsValidSecret(HttpRequest request)
    {
        var expected = _configuration["Telegram:WebhookSecret"];
        if (string.IsNullOrEmpty(expected))
        {
            _logger.LogWarning("Telegram:WebhookSecret не настроен. Запрос отклонён.");
            return false;
        }

        var actual = request.Headers["X-Telegram-Bot-Api-Secret-Token"].ToString();
        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        var actualBytes = Encoding.UTF8.GetBytes(actual);

        return CryptographicOperations.FixedTimeEquals(expectedBytes, actualBytes);
    }
}
