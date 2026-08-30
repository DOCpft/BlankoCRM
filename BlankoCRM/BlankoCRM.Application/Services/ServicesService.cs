using BlankoCRM.Application.Abstract;
using BlankoCRM.Core.Entities;
using BlankoCRM.Infrastructure;
using BlankoCRM.Infrastructure.Data;
using BlankoCRM.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BlankoCRM.Application.Services;

/// <summary>
/// Реализация сервиса для работы с услугами.
/// </summary>
public class ServicesService : IServicesService
{
    private readonly AppDbContext _dbContext;

    public ServicesService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Service>> GetAllAsync()
    {
        return await _dbContext.Services
            .OrderBy(s => s.Name)
            .ToListAsync();
    }

    public async Task<Service?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Services.FindOrThrowAsync(id);
    }

    public async Task<Service> CreateAsync(string name, int durationMinutes, decimal price, string? description)
    {
        var now = DateTime.UtcNow;

        var service = new Service
        {
            Id = Guid.NewGuid(),
            TenantId = TenantContext.CurrentTenantId!.Value,
            Name = name.Trim(),
            DurationMinutes = durationMinutes,
            Price = price,
            Description = description,
            CreatedAt = now
        };

        _dbContext.Services.Add(service);
        await _dbContext.SaveChangesAsync();

        return service;
    }

    public async Task UpdateAsync(Guid id, string? name, int? durationMinutes, decimal? price, string? description)
    {
        var service = await _dbContext.Services.FindOrThrowAsync(id);


        service.Name = name == null ? service.Name : name.Trim();
        service.DurationMinutes = durationMinutes.HasValue ? durationMinutes.Value : service.DurationMinutes;
        service.Price = price.HasValue ? price.Value : service.Price;
        service.Description = description == null ? service.Description : description;
        service.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var service = await _dbContext.Services
            .Include(s => s.AppointmentServices)
            .FirstOrDefaultAsync(s => s.Id == id)
            ?? throw new KeyNotFoundException($"Услуга с ID {id} не найдена");

        if (service.AppointmentServices is { Count: > 0 })
        {
            throw new InvalidOperationException("Нельзя удалить услугу, которая используется в записях");
        }

        _dbContext.Services.Remove(service);
        await _dbContext.SaveChangesAsync();
    }
}
