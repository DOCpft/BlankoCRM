using BlankoCRM.Core.Entities;

namespace BlankoCRM.Core.Interfaces;

/// <summary>
/// Репозиторий слотов расписания.
/// </summary>
public interface IScheduleRepository : IRepository<Schedule>
{
    Task<Schedule?> GetByIdWithAppointmentAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Schedule?> GetFreeSlotByIdAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default);
    Task LockFreeSlotForUpdateAsync(Guid organizationId, Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Schedule>> GetFreeSlotsByPeriodAsync(
        Guid organizationId,
        DateTime dateFrom,
        DateTime dateTo,
        int limit,
        CancellationToken cancellationToken = default);
    Task<Schedule?> GetSlotByAppointmentIdAsync(Guid appointmentId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Schedule>> GetByPeriodAsync(DateTime dateFrom, DateTime dateTo, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<Schedule> entities, CancellationToken cancellationToken = default);
    Task<bool> HasOverlapWithFreeSlotsAsync(
        DateTime startTime,
        DateTime endTime,
        Guid organizationId,
        Guid? salonId = null,
        Guid? excludedSlotId = null,
        CancellationToken cancellationToken = default);
    Task<int> DeleteFreeSlotsByTimeAsync(
        DateTime startTime,
        DateTime endTime,
        Guid organizationId,
        Guid? salonId = null,
        Guid? excludedSlotId = null,
        CancellationToken cancellationToken = default);
}
