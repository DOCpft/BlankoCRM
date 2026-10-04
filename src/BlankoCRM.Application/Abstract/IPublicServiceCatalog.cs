using BlankoCRM.Core.Entities;

namespace BlankoCRM.Application.Abstract;

public interface IPublicServiceCatalog
{
    Task<IReadOnlyList<Service>> GetServicesAsync(CancellationToken cancellationToken = default);
}
