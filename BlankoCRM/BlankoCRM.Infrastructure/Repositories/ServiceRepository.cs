using BlankoCRM.Core.Entities;
using BlankoCRM.Core.Interfaces;
using BlankoCRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BlankoCRM.Infrastructure.Repositories;

/// <summary>
/// EF Core репозиторий услуг.
/// </summary>
public sealed class ServiceRepository : EfRepository<Service>, IServiceRepository
{
    public ServiceRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Service?> GetByIdWithAppointmentServicesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbContext.Services
            .Include(service => service.AppointmentServices)
            .FirstOrDefaultAsync(service => service.Id == id, cancellationToken);
    }
}
