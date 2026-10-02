using BlankoCRM.Application.Abstract;
using BlankoCRM.Core.Entities;
using BlankoCRM.Core.Interfaces;
using BlankoCRM.Core.Exceptions;

namespace BlankoCRM.Application.Services;

/// <summary>
/// Р РµР°Р»РёР·Р°С†РёСЏ СЃРµСЂРІРёСЃР° РґР»СЏ СЂР°Р±РѕС‚С‹ СЃ СѓСЃР»СѓРіР°РјРё.
/// </summary>
public class ServicesService : IServicesService
{
    private readonly IServiceRepository _serviceRepository;
    private readonly IRepository<Salon> _salonRepository;
    private readonly IOrganizationContext _organizationContext;
    private readonly IUnitOfWork _unitOfWork;

    public ServicesService(
        IServiceRepository serviceRepository,
        IRepository<Salon> salonRepository,
        IOrganizationContext organizationContext,
        IUnitOfWork unitOfWork)
    {
        _serviceRepository = serviceRepository;
        _salonRepository = salonRepository;
        _organizationContext = organizationContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<Service>> GetAllAsync()
    {
        var services = await _serviceRepository.GetAllAsync();
        return services.OrderBy(service => service.Name).ToList();
    }

    public async Task<Service?> GetByIdAsync(Guid id)
    {
        return await _serviceRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Service with ID {id} was not found.");
    }

    public async Task<Service> CreateAsync(string name, int durationMinutes, decimal price, string? description, Guid? salonId = null)
    {
        await EnsureSalonBelongsToOrganizationAsync(salonId);
        var now = DateTime.UtcNow;

        var service = new Service
        {
            Id = Guid.NewGuid(),
            OrganizationId = _organizationContext.GetCurrentOrganizationId()!.Value,
            Name = name.Trim(),
            DurationMinutes = durationMinutes,
            Price = price,
            Description = description,
            SalonId = salonId,
            CreatedAt = now
        };

        var ownsTransaction = !_unitOfWork.HasActiveTransaction;
        await using var transaction = ownsTransaction
            ? await _unitOfWork.BeginTransactionAsync()
            : null;

        await _serviceRepository.AddAsync(service);
        await _unitOfWork.SaveChangesAsync();
        if (ownsTransaction)
        {
            await transaction!.CommitAsync();
        }

        return service;
    }

    public async Task UpdateAsync(Guid id, string? name, int? durationMinutes, decimal? price, string? description, Guid? salonId = null)
    {
        var service = await _serviceRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Service with ID {id} was not found.");

        await EnsureSalonBelongsToOrganizationAsync(salonId);


        service.Name = name == null ? service.Name : name.Trim();
        service.DurationMinutes = durationMinutes.HasValue ? durationMinutes.Value : service.DurationMinutes;
        service.Price = price.HasValue ? price.Value : service.Price;
        service.Description = description == null ? service.Description : description;
        if (salonId.HasValue)
        {
            service.SalonId = salonId;
        }
        service.UpdatedAt = DateTime.UtcNow;

        var ownsTransaction = !_unitOfWork.HasActiveTransaction;
        await using var transaction = ownsTransaction
            ? await _unitOfWork.BeginTransactionAsync()
            : null;

        await _serviceRepository.UpdateAsync(service);
        await _unitOfWork.SaveChangesAsync();
        if (ownsTransaction)
        {
            await transaction!.CommitAsync();
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        var service = await _serviceRepository.GetByIdWithAppointmentServicesAsync(id)
            ?? throw new NotFoundException($"Service with ID {id} was not found.");

        if (service.AppointmentServices is { Count: > 0 })
        {
            throw new ConflictException("Service cannot be deleted because it is used by appointments.");
        }

        var ownsTransaction = !_unitOfWork.HasActiveTransaction;
        await using var transaction = ownsTransaction
            ? await _unitOfWork.BeginTransactionAsync()
            : null;

        await _serviceRepository.DeleteAsync(service);
        await _unitOfWork.SaveChangesAsync();
        if (ownsTransaction)
        {
            await transaction!.CommitAsync();
        }
    }

    private async Task EnsureSalonBelongsToOrganizationAsync(Guid? salonId)
    {
        if (salonId.HasValue && await _salonRepository.GetByIdAsync(salonId.Value) is null)
        {
            throw new NotFoundException($"Salon with ID {salonId.Value} was not found in the current organization.");
        }
    }
}

