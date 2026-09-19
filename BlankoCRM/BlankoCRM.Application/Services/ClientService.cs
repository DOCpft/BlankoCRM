using BlankoCRM.Application.Abstract;
using BlankoCRM.Application.DTOs;
using BlankoCRM.Core.Entities;
using BlankoCRM.Core.Interfaces;
using BlankoCRM.Core.Exceptions;

namespace BlankoCRM.Application.Services;

public class ClientService : IClientService
{
    private readonly IClientRepository _clientRepository;
    private readonly IRepository<Salon> _salonRepository;
    private readonly IOrganizationContext _organizationContext;
    private readonly IUnitOfWork _unitOfWork;

    public ClientService(
        IClientRepository clientRepository,
        IRepository<Salon> salonRepository,
        IOrganizationContext organizationContext,
        IUnitOfWork unitOfWork)
    {
        _clientRepository = clientRepository;
        _salonRepository = salonRepository;
        _organizationContext = organizationContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<Client>> GetAllAsync(string? search)
    {
        var clients = string.IsNullOrWhiteSpace(search)
            ? await _clientRepository.GetAllWithContactsAsync()
            : await _clientRepository.FindWithContactsAsync(client =>
                client.Name.Contains(search.Trim()) || (client.Phone != null && client.Phone.Contains(search.Trim())));

        return clients.OrderBy(client => client.Name).ToList();
    }

    public async Task<Client?> GetByIdAsync(Guid id)
    {
        return await _clientRepository.GetByIdWithContactsAsync(id)
            ?? throw new NotFoundException($"Client with ID {id} was not found.");
    }

    public async Task<Client> CreateAsync(
        string name,
        string? phone,
        IReadOnlyCollection<ClientContactDto>? contacts,
        DateTime? birthDate,
        string? notes,
        Guid? salonId = null)
    {
        var organizationId = GetOrganizationId();
        await EnsureSalonBelongsToOrganizationAsync(salonId);
        var now = DateTime.UtcNow;

        var client = new Client
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Phone = Normalize(phone),
            BirthDate = birthDate,
            Notes = notes,
            CreatedAt = now,
            OrganizationId = organizationId,
            SalonId = salonId,
            Contacts = contacts?.Select(contact => new ClientContact
            {
                Id = Guid.NewGuid(),
                ClientId =  Guid.Empty,
                OrganizationId = organizationId,
                Type = contact.Type,
                Value = contact.Value.Trim(),
                CreatedAt = now
            }).ToList() ?? []
        };

        foreach (var contact in client.Contacts)
        {
            contact.ClientId = client.Id;
        }

        return await _clientRepository.AddAsync(client);
    }

    public async Task UpdateAsync(
        Guid id,
        string? name,
        string? phone,
        IReadOnlyCollection<ClientContactDto>? contacts,
        DateTime? birthDate,
        string? notes,
        Guid? salonId = null)
    {
        var client = await _clientRepository.GetByIdWithContactsAsync(id)
            ?? throw new NotFoundException($"Client with ID {id} was not found.");

        await EnsureSalonBelongsToOrganizationAsync(salonId);

        client.Name = name == null ? client.Name : name.Trim();
        client.Phone = phone == null ? client.Phone : Normalize(phone);
        client.BirthDate = birthDate ?? client.BirthDate;
        client.Notes = notes ?? client.Notes;
        if (salonId.HasValue)
        {
            client.SalonId = salonId;
        }

        if (contacts is not null)
        {
            SynchronizeContacts(client, contacts);
        }

        client.UpdatedAt = DateTime.UtcNow;

        await using var transaction = await _unitOfWork.BeginTransactionAsync();
        await _unitOfWork.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var client = await _clientRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Client with ID {id} was not found.");

        await _clientRepository.DeleteAsync(client);
    }

    private void SynchronizeContacts(Client client, IReadOnlyCollection<ClientContactDto> requestedContacts)
    {
        var requestedByType = requestedContacts.ToDictionary(contact => contact.Type);
        var existingByType = client.Contacts.ToDictionary(contact => contact.Type);

        foreach (var existingContact in client.Contacts.ToList())
        {
            if (!requestedByType.ContainsKey(existingContact.Type))
            {
                _unitOfWork.Remove(existingContact);
            }
        }

        foreach (var requestedContact in requestedContacts)
        {
            var value = requestedContact.Value.Trim();
            if (existingByType.TryGetValue(requestedContact.Type, out var existingContact))
            {
                if (!string.Equals(existingContact.Value, value, StringComparison.Ordinal))
                {
                    existingContact.Value = value;
                    existingContact.UpdatedAt = DateTime.UtcNow;
                }

                continue;
            }

            var newContact = new ClientContact
            {
                Id = Guid.NewGuid(),
                ClientId = client.Id,
                OrganizationId = client.OrganizationId,
                Type = requestedContact.Type,
                Value = value,
                CreatedAt = DateTime.UtcNow
            };

            client.Contacts.Add(newContact);
            _unitOfWork.Add(newContact);
        }
    }

    private async Task EnsureSalonBelongsToOrganizationAsync(Guid? salonId)
    {
        if (salonId.HasValue && await _salonRepository.GetByIdAsync(salonId.Value) is null)
        {
            throw new NotFoundException($"Salon with ID {salonId.Value} was not found in the current organization.");
        }
    }

    private Guid GetOrganizationId() => _organizationContext.GetCurrentOrganizationId()
        ?? throw new UnauthorizedAccessException("Organization context is not initialized.");

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

