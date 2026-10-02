using BlankoCRM.Application.Abstract;
using BlankoCRM.Core.Entities;
using BlankoCRM.Core.Exceptions;
using BlankoCRM.Core.Interfaces;
using System.Globalization;

namespace BlankoCRM.Application.Services;

/// <summary>
/// Разрешает карточку клиента для публичной заявки без автоматического объединения карточек.
/// </summary>
public sealed class PublicClientResolver : IPublicClientResolver
{
    private readonly IClientRepository _clientRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PublicClientResolver(IClientRepository clientRepository, IUnitOfWork unitOfWork)
    {
        _clientRepository = clientRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Client> ResolveAsync(
        Guid organizationId,
        long verifiedVkUserId,
        string name,
        string? phone,
        CancellationToken cancellationToken = default)
    {
        if (organizationId == Guid.Empty)
        {
            throw new UnauthorizedAccessException("Organization context is not initialized.");
        }

        if (verifiedVkUserId <= 0)
        {
            throw new ValidationException("Некорректный подтверждённый VK ID.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ValidationException("Имя клиента обязательно.");
        }

        var normalizedPhone = PhoneNumberNormalizer.Normalize(phone);
        var clientByVk = await _clientRepository.FindByVkUserIdAsync(
            organizationId, verifiedVkUserId, cancellationToken);
        var clientByPhone = normalizedPhone is null
            ? null
            : await _clientRepository.FindByPhoneAsync(organizationId, normalizedPhone, cancellationToken);

        if (clientByVk is not null && clientByPhone is not null && clientByVk.Id != clientByPhone.Id)
        {
            throw new ConflictException(
                "VK ID и телефон связаны с разными карточками клиентов. Уточните данные клиента.");
        }

        var now = DateTime.UtcNow;
        var vkValue = verifiedVkUserId.ToString(CultureInfo.InvariantCulture);

        if (clientByVk is not null)
        {
            if (normalizedPhone is not null)
            {
                if (clientByVk.Phone is not null && clientByVk.Phone != normalizedPhone)
                {
                    throw new ConflictException(
                        "У клиента уже указан другой телефон. Уточните данные клиента.");
                }

                if (clientByVk.Phone is null)
                {
                    clientByVk.Phone = normalizedPhone;
                    clientByVk.UpdatedAt = now;
                }
            }

            return clientByVk;
        }

        if (clientByPhone is not null)
        {
            var existingVkContact = clientByPhone.Contacts
                .FirstOrDefault(contact => contact.Type == ContactType.Vk);

            if (existingVkContact is not null && existingVkContact.Value != vkValue)
            {
                throw new ConflictException(
                    "У клиента уже привязан другой VK ID. Уточните данные клиента.");
            }

            if (existingVkContact is null)
            {
                var contact = new ClientContact
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    ClientId = clientByPhone.Id,
                    Type = ContactType.Vk,
                    Value = vkValue,
                    CreatedAt = now
                };
                clientByPhone.Contacts.Add(contact);
                _unitOfWork.Add(contact);
                clientByPhone.UpdatedAt = now;
            }

            return clientByPhone;
        }

        var newClient = new Client
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            Name = name.Trim(),
            Phone = normalizedPhone,
            CreatedAt = now,
            Contacts =
            [
                new ClientContact
                {
                    Id = Guid.NewGuid(),
                    OrganizationId = organizationId,
                    Type = ContactType.Vk,
                    Value = vkValue,
                    CreatedAt = now
                }
            ]
        };
        newClient.Contacts.First().ClientId = newClient.Id;

        await _clientRepository.AddAsync(newClient, cancellationToken);
        return newClient;
    }
}
