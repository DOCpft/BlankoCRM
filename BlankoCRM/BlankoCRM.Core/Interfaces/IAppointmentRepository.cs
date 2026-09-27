using BlankoCRM.Core.Entities;

namespace BlankoCRM.Core.Interfaces;

/// <summary>
/// Репозиторий записей с запросами связанных данных.
/// </summary>
public interface IAppointmentRepository : IRepository<Appointment>
{
    Task<Appointment?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task LockForUpdateAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Appointment>> GetForDayAsync(DateTime date, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Appointment>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Appointment>> GetActivePendingAsync(
        Guid organizationId,
        DateTime now,
        CancellationToken cancellationToken = default);
    Task<int> CountActivePendingAsync(
        Guid organizationId,
        Guid clientId,
        DateTime now,
        CancellationToken cancellationToken = default);
    Task<int> CancelExpiredPendingBatchAsync(
        DateTime now,
        int batchSize,
        CancellationToken cancellationToken = default);
    Task<bool> HasOverlapAsync(
        DateTime startTime,
        DateTime endTime,
        Guid organizationId,
        Guid? salonId = null,
        Guid? excludedAppointmentId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Возвращает записи со статусом Scheduled, попадающие в заданный временной интервал,
    /// вместе с данными клиента и услугами. Используется фоновым сервисом напоминаний
    /// и поэтому игнорирует глобальный фильтр мультитенантности.
    /// </summary>
    Task<IReadOnlyList<Appointment>> GetScheduledInWindowAsync(
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default);
}
