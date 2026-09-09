using BlankoCRM.Core.Entities;

namespace BlankoCRM.Core.Interfaces;

/// <summary>
/// Репозиторий записей с запросами связанных данных.
/// </summary>
public interface IAppointmentRepository : IRepository<Appointment>
{
    Task<Appointment?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Appointment>> GetForDayAsync(DateTime date, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Appointment>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default);
    Task<bool> HasOverlapAsync(
        DateTime startTime,
        DateTime endTime,
        Guid organizationId,
        Guid? salonId = null,
        Guid? excludedAppointmentId = null,
        CancellationToken cancellationToken = default);
}
