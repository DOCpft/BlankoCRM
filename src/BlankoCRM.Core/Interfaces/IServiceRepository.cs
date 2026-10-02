using BlankoCRM.Core.Entities;

namespace BlankoCRM.Core.Interfaces;

/// <summary>
/// Репозиторий услуг с загрузкой связанных записей.
/// </summary>
public interface IServiceRepository : IRepository<Service>
{
    Task<Service?> GetByIdWithAppointmentServicesAsync(Guid id, CancellationToken cancellationToken = default);
}
