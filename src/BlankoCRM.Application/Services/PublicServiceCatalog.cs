using BlankoCRM.Application.Abstract;
using BlankoCRM.Core.Entities;
using BlankoCRM.Core.Interfaces;

namespace BlankoCRM.Application.Services;

public sealed class PublicServiceCatalog : IPublicServiceCatalog
{
    private readonly IRepository<Service> _serviceRepository;

    public PublicServiceCatalog(IRepository<Service> serviceRepository)
    {
        _serviceRepository = serviceRepository;
    }

    public async Task<IReadOnlyList<Service>> GetServicesAsync(CancellationToken cancellationToken = default)
    {
        var services = await _serviceRepository.GetAllAsync(cancellationToken);
        return services.OrderBy(service => service.Name).ToList();
    }
}
