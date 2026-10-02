using BlankoCRM.Application.Abstract;
using BlankoCRM.Core.Entities;
using BlankoCRM.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BlankoCRM.Application.Services;

/// <summary>
/// Находит записи, для которых подошло время напоминания, и отправляет напоминания мастерам.
/// </summary>
public sealed class ReminderDispatchService : IReminderDispatchService
{
    private static readonly int[] Offsets = [1, 3, 6, 12, 24];
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(5);

    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IMasterReminderSettingRepository _settingRepository;
    private readonly IAppointmentReminderLogRepository _logRepository;
    private readonly ITelegramNotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<AppUser> _userManager;
    private readonly ILogger<ReminderDispatchService> _logger;

    public ReminderDispatchService(
        IAppointmentRepository appointmentRepository,
        IMasterReminderSettingRepository settingRepository,
        IAppointmentReminderLogRepository logRepository,
        ITelegramNotificationService notificationService,
        IUnitOfWork unitOfWork,
        UserManager<AppUser> userManager,
        ILogger<ReminderDispatchService> logger)
    {
        _appointmentRepository = appointmentRepository;
        _settingRepository = settingRepository;
        _logRepository = logRepository;
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<int> DispatchDueRemindersAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var sent = 0;

        foreach (var offsetHours in Offsets)
        {
            var fromUtc = now.AddHours(offsetHours).Subtract(Window);
            var toUtc = now.AddHours(offsetHours).Add(Window);

            var appointments = await _appointmentRepository.GetScheduledInWindowAsync(fromUtc, toUtc, ct);

            foreach (var appointment in appointments)
            {
                if (await TrySendReminderAsync(appointment, offsetHours, ct))
                {
                    sent++;
                }
            }
        }

        return sent;
    }

    private async Task<bool> TrySendReminderAsync(Appointment appointment, int offsetHours, CancellationToken ct)
    {
        try
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(
                u => u.OrganizationId == appointment.OrganizationId,
                ct);
            if (user is null)
            {
                return false;
            }

            if (!await _settingRepository.IsEnabledAsync(user.Id, offsetHours, ct))
            {
                return false;
            }

            if (await _logRepository.ExistsAsync(appointment.Id, offsetHours, ct))
            {
                return false;
            }

            await _notificationService.SendReminderAsync(appointment, offsetHours, ct);

            var log = new AppointmentReminderLog
            {
                Id = Guid.NewGuid(),
                OrganizationId = appointment.OrganizationId,
                AppointmentId = appointment.Id,
                OffsetHours = offsetHours,
                SentAt = DateTime.UtcNow
            };

            var ownsTransaction = !_unitOfWork.HasActiveTransaction;
            await using var transaction = ownsTransaction ? await _unitOfWork.BeginTransactionAsync(ct) : null;
            try
            {
                await _logRepository.AddAsync(log, ct);
                await _unitOfWork.SaveChangesAsync(ct);
                if (ownsTransaction)
                {
                    await transaction!.CommitAsync(ct);
                }
            }
            catch
            {
                if (ownsTransaction && _unitOfWork.HasActiveTransaction)
                {
                    await transaction!.RollbackAsync(ct);
                }

                throw;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Не удалось отправить напоминание для записи {AppointmentId} за {OffsetHours} ч.",
                appointment.Id,
                offsetHours);
            return false;
        }
    }
}
