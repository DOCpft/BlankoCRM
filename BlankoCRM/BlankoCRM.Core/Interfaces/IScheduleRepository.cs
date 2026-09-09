using BlankoCRM.Core.Entities;

namespace BlankoCRM.Core.Interfaces;

/// <summary>
/// Репозиторий слотов расписания.
/// </summary>
public interface IScheduleRepository : IRepository<Schedule>
{
    Task<Schedule?> GetByIdWithAppointmentAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Schedule>> GetByPeriodAsync(DateTime dateFrom, DateTime dateTo, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<Schedule> entities, CancellationToken cancellationToken = default);
}
