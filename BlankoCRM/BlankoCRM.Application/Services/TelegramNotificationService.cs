using BlankoCRM.Application.Abstract;
using BlankoCRM.Core.Entities;
using BlankoCRM.Core.Exceptions;
using BlankoCRM.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlankoCRM.Application.Services;

/// <summary>
/// Формирует и отправляет Telegram-уведомления мастеру. Ошибки не пробрасывает,
/// чтобы уведомления не ломали основной бизнес-флоу.
/// </summary>
public sealed class TelegramNotificationService : ITelegramNotificationService
{
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly UserManager<AppUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TelegramNotificationService> _logger;

    public TelegramNotificationService(
        ITelegramBotClient telegramBotClient,
        UserManager<AppUser> userManager,
        IUnitOfWork unitOfWork,
        ILogger<TelegramNotificationService> logger)
    {
        _telegramBotClient = telegramBotClient;
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task NotifyMasterNewPendingAsync(Appointment appointment, CancellationToken ct = default)
    {
        try
        {
            var user = await GetMasterAsync(appointment.OrganizationId, ct);
            if (user is null || !user.TelegramChatId.HasValue || !user.TelegramNotificationsEnabled)
            {
                return;
            }

            var text = BuildNewPendingMessage(appointment, user);
            await _telegramBotClient.SendMessageAsync(user.TelegramChatId.Value, text, ct);
        }
        catch (TelegramForbiddenException ex)
        {
            _logger.LogWarning(
                ex,
                "Telegram-бот заблокирован мастером организации {OrganizationId}. Сбрасываем TelegramChatId.",
                appointment.OrganizationId);
            await ResetTelegramChatIdAsync(appointment.OrganizationId, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Не удалось уведомить мастера организации {OrganizationId} о новой заявке.",
                appointment.OrganizationId);
        }
    }

    public async Task SendReminderAsync(Appointment appointment, int offsetHours, CancellationToken ct = default)
    {
        try
        {
            var user = await GetMasterAsync(appointment.OrganizationId, ct);
            if (user is null || !user.TelegramChatId.HasValue || !user.TelegramNotificationsEnabled)
            {
                return;
            }

            var text = BuildReminderMessage(appointment, offsetHours, user);
            await _telegramBotClient.SendMessageAsync(user.TelegramChatId.Value, text, ct);
        }
        catch (TelegramForbiddenException ex)
        {
            _logger.LogWarning(
                ex,
                "Telegram-бот заблокирован мастером организации {OrganizationId}. Сбрасываем TelegramChatId.",
                appointment.OrganizationId);
            await ResetTelegramChatIdAsync(appointment.OrganizationId, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Не удалось отправить напоминание мастеру организации {OrganizationId} за {OffsetHours} ч.",
                appointment.OrganizationId,
                offsetHours);
        }
    }

    private Task<AppUser?> GetMasterAsync(Guid organizationId, CancellationToken ct)
    {
        return _userManager.Users.FirstOrDefaultAsync(user => user.OrganizationId == organizationId, ct);
    }

    private async Task ResetTelegramChatIdAsync(Guid organizationId, CancellationToken ct)
    {
        try
        {
            var user = await GetMasterAsync(organizationId, ct);
            if (user is not null && user.TelegramChatId.HasValue)
            {
                user.TelegramChatId = null;
                await _unitOfWork.SaveChangesAsync(ct);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Не удалось сбросить TelegramChatId мастера организации {OrganizationId}.", organizationId);
        }
    }

    private static string BuildNewPendingMessage(Appointment appointment, AppUser user)
    {
        var services = BuildServicesList(appointment);
        var time = DateTimeFormatter.FormatInTimeZone(appointment.StartTime, user.TimeZone);
        var notes = string.IsNullOrWhiteSpace(appointment.Notes) ? "—" : appointment.Notes;

        return $"📅 Новая заявка от {ClientName(appointment)}\n"
             + $"Услуги: {services}\n"
             + $"Время: {time}\n"
             + $"Заметки: {notes}";
    }

    private static string BuildReminderMessage(Appointment appointment, int offsetHours, AppUser user)
    {
        var services = BuildServicesList(appointment);
        var time = DateTimeFormatter.FormatInTimeZone(appointment.StartTime, user.TimeZone);

        return $"⏰ Напоминание: через {offsetHours} ч. запись\n"
             + $"Клиент: {ClientName(appointment)}\n"
             + $"Услуги: {services}\n"
             + $"Время: {time}";
    }

    private static string BuildServicesList(Appointment appointment)
    {
        var names = appointment.AppointmentServices
            .Select(service => service.ServiceName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToList();

        return names.Count == 0 ? "—" : string.Join(", ", names);
    }

    private static string ClientName(Appointment appointment)
    {
        return string.IsNullOrWhiteSpace(appointment.Client?.Name) ? "—" : appointment.Client.Name;
    }
}
