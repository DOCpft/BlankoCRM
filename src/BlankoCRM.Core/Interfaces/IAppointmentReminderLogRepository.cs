using BlankoCRM.Core.Entities;

namespace BlankoCRM.Core.Interfaces;

/// <summary>
/// Репозиторий журнала отправленных напоминаний.
/// </summary>
public interface IAppointmentReminderLogRepository : IRepository<AppointmentReminderLog>
{
    /// <summary>
    /// Проверяет, было ли уже отправлено напоминание для пары (запись, offset).
    /// </summary>
    Task<bool> ExistsAsync(
        Guid appointmentId,
        int offsetHours,
        CancellationToken cancellationToken = default);
}
