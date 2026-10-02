using BlankoCRM.Application.Abstract;
using BlankoCRM.Application.DTOs;
using BlankoCRM.Core.Entities;
using BlankoCRM.Core.Exceptions;
using BlankoCRM.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BlankoCRM.Web.Controllers;

/// <summary>
/// Профиль текущего мастера: привязка Telegram, настройки напоминаний и часовой пояс.
/// </summary>
[ApiController]
[Route("api/profile")]
[Authorize]
public sealed class ProfileController : ControllerBase
{
    private readonly ITelegramLinkService _telegramLinkService;
    private readonly IReminderSettingService _reminderSettingService;
    private readonly UserManager<AppUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;

    public ProfileController(
        ITelegramLinkService telegramLinkService,
        IReminderSettingService reminderSettingService,
        UserManager<AppUser> userManager,
        IUnitOfWork unitOfWork)
    {
        _telegramLinkService = telegramLinkService;
        _reminderSettingService = reminderSettingService;
        _userManager = userManager;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Сгенерировать ссылку для привязки Telegram-аккаунта.
    /// </summary>
    [HttpPost("telegram/link")]
    [ProducesResponseType(typeof(TelegramLinkResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TelegramLinkResponse>> GenerateTelegramLink(CancellationToken ct)
    {
        return Ok(await _telegramLinkService.GenerateLinkAsync(ct));
    }

    /// <summary>
    /// Отвязать Telegram-аккаунт.
    /// </summary>
    [HttpDelete("telegram/link")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UnlinkTelegram(CancellationToken ct)
    {
        await _telegramLinkService.UnlinkAsync(ct);
        return NoContent();
    }

    /// <summary>
    /// Получить статус привязки Telegram-аккаунта.
    /// </summary>
    [HttpGet("telegram/status")]
    [ProducesResponseType(typeof(TelegramLinkStatus), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TelegramLinkStatus>> GetTelegramStatus(CancellationToken ct)
    {
        return Ok(await _telegramLinkService.GetStatusAsync(ct));
    }

    /// <summary>
    /// Получить настройки напоминаний текущего мастера.
    /// </summary>
    [HttpGet("reminder-settings")]
    [ProducesResponseType(typeof(List<ReminderSettingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<List<ReminderSettingDto>>> GetReminderSettings(CancellationToken ct)
    {
        return Ok(await _reminderSettingService.GetAllAsync(ct));
    }

    /// <summary>
    /// Обновить настройки напоминаний.
    /// </summary>
    [HttpPut("reminder-settings")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateReminderSettings(
        [FromBody] UpdateReminderSettingsRequest request,
        CancellationToken ct)
    {
        await _reminderSettingService.UpdateAsync(request, ct);
        return NoContent();
    }

    /// <summary>
    /// Изменить часовой пояс мастера.
    /// </summary>
    [HttpPatch("timezone")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateTimeZone(
        [FromBody] UpdateTimeZoneRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.TimeZone))
        {
            throw new ValidationException("Часовой пояс обязателен.");
        }

        try
        {
            _ = TimeZoneInfo.FindSystemTimeZoneById(request.TimeZone);
        }
        catch (Exception exception) when (
            exception is TimeZoneNotFoundException or InvalidTimeZoneException or ArgumentException)
        {
            throw new ValidationException($"Некорректный часовой пояс: {request.TimeZone}.");
        }

        var user = await _userManager.GetUserAsync(User)
            ?? throw new UnauthorizedException("Пользователь не найден.");

        user.TimeZone = request.TimeZone;
        await _unitOfWork.SaveChangesAsync(ct); // Единственное нарушение стиля проекта. Чтоб не плодить сервис ради одного метода оставим тут. Осознанный компромисс.

        return NoContent();
    }
}
