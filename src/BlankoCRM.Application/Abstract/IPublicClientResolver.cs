using BlankoCRM.Core.Entities;

namespace BlankoCRM.Application.Abstract;

/// <summary>
/// Находит или подготавливает клиента для публичной заявки по подтверждённому VK ID и телефону.
/// Изменения сохраняются вызывающей операцией в её транзакции.
/// </summary>
public interface IPublicClientResolver
{
    Task<Client> ResolveAsync(
        Guid organizationId,
        long verifiedVkUserId,
        string name,
        string? phone,
        CancellationToken cancellationToken = default);
}
