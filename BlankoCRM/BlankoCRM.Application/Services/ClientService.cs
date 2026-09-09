using BlankoCRM.Application.Abstract;
using BlankoCRM.Core.Entities;
using BlankoCRM.Core.Interfaces;
using BlankoCRM.Core.Exceptions;

namespace BlankoCRM.Application.Services;

/// <summary>
/// Р РµР°Р»РёР·Р°С†РёСЏ СЃРµСЂРІРёСЃР° РґР»СЏ СЂР°Р±РѕС‚С‹ СЃ РєР»РёРµРЅС‚Р°РјРё.
/// </summary>
public class ClientService : IClientService
{
    private readonly IRepository<Client> _clientRepository;
    private readonly IRepository<Salon> _salonRepository;
    private readonly IOrganizationContext _organizationContext;

    public ClientService(
        IRepository<Client> clientRepository,
        IRepository<Salon> salonRepository,
        IOrganizationContext organizationContext)
    {
        _clientRepository = clientRepository;
        _salonRepository = salonRepository;
        _organizationContext = organizationContext;
    }

    public async Task<List<Client>> GetAllAsync(string? search)
    {
        var clients = string.IsNullOrWhiteSpace(search)
            ? await _clientRepository.GetAllAsync()
            : await _clientRepository.FindAsync(client =>
                client.Name.Contains(search.Trim()) || client.Phone.Contains(search.Trim()));

        return clients.OrderBy(client => client.Name).ToList();
    }

    public async Task<Client?> GetByIdAsync(Guid id)
    {
        return await _clientRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Client with ID {id} was not found.");
    }

    public async Task<Client> CreateAsync(string name, string phone, string? telegram, DateTime? birthDate, string? notes, Guid? salonId = null)
    {
        await EnsureSalonBelongsToOrganizationAsync(salonId);
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
            OrganizationId = _organizationContext.GetCurrentOrganizationId()!.Value,
            SalonId = salonId,
        };

        return await _clientRepository.AddAsync(client);
    }

    public async Task UpdateAsync(Guid id, string? name, string? phone, string? telegram, DateTime? birthDate, string? notes, Guid? salonId = null)
    {
        var client = await _clientRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Client with ID {id} was not found.");

        await EnsureSalonBelongsToOrganizationAsync(salonId);

        client.Name = name == null ? client.Name : name.Trim();
        client.Phone = phone == null ? client.Phone : phone.Trim();
        client.Telegram = telegram == null ? client.Telegram : telegram.Trim();
        client.BirthDate = birthDate.HasValue ? birthDate : client.BirthDate;
        client.Notes = notes == null ? client.Notes : notes;
        if (salonId.HasValue)
        {
            client.SalonId = salonId;
        }
        client.UpdatedAt = DateTime.UtcNow;

        await _clientRepository.UpdateAsync(client);
    }

    public async Task DeleteAsync(Guid id)
    {
        var client = await _clientRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Client with ID {id} was not found.");

        await _clientRepository.DeleteAsync(client);
    }

    private async Task EnsureSalonBelongsToOrganizationAsync(Guid? salonId)
    {
        if (salonId.HasValue && await _salonRepository.GetByIdAsync(salonId.Value) is null)
        {
            throw new NotFoundException($"Salon with ID {salonId.Value} was not found in the current organization.");
        }
    }
}

