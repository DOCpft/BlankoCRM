using BlankoCRM.Application.Abstract;
using BlankoCRM.Core.Entities;
using BlankoCRM.Infrastructure;
using BlankoCRM.Infrastructure.Data;
using BlankoCRM.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BlankoCRM.Application.Services;

/// <summary>
/// Реализация сервиса для работы с клиентами.
/// </summary>
public class ClientService : IClientService
{
    private readonly AppDbContext _dbContext;

    public ClientService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Client>> GetAllAsync(string? search)
    {
        var query = _dbContext.Clients.AsQueryable();

        // Выгружаем ВСЕХ клиентов у которых Имя или Телефон такие-то (в дальнейшем наверное будем добавлять поля)
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(c =>
                c.Name.Contains(term) ||
                c.Phone.Contains(term));
        }

        return await query
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Client?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Clients.FindOrThrowAsync(id);
    }

    public async Task<Client> CreateAsync(string name, string phone, string? telegram, DateTime? birthDate, string? notes)
    {
        var now = DateTime.UtcNow;

        var client = new Client
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Phone = phone.Trim(),
            Telegram = telegram?.Trim(),
            BirthDate = birthDate,
            Notes = notes,
            CreatedAt = now,
            TenantId = TenantContext.CurrentTenantId!.Value,
        };

        _dbContext.Clients.Add(client);
        await _dbContext.SaveChangesAsync();

        return client;
    }

    public async Task UpdateAsync(Guid id, string? name, string? phone, string? telegram, DateTime? birthDate, string? notes)
    {
        var client = await _dbContext.Clients.FindOrThrowAsync(id);

        client.Name = name == null ? client.Name : name.Trim();
        client.Phone = phone == null ? client.Phone : phone.Trim();
        client.Telegram = telegram == null ? client.Telegram : telegram.Trim();
        client.BirthDate = birthDate.HasValue ? birthDate : client.BirthDate;
        client.Notes = notes == null ? client.Notes : notes;
        client.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var client = await _dbContext.Clients.FindOrThrowAsync(id);

        _dbContext.Clients.Remove(client);
        await _dbContext.SaveChangesAsync();
    }
}
