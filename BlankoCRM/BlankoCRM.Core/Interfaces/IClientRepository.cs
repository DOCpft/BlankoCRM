using System.Linq.Expressions;
using BlankoCRM.Core.Entities;

namespace BlankoCRM.Core.Interfaces;

public interface IClientRepository : IRepository<Client>
{
    Task<Client?> GetByIdWithContactsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Client?> FindByPhoneAsync(Guid organizationId, string normalizedPhone, CancellationToken cancellationToken = default);
    Task<Client?> FindByVkUserIdAsync(Guid organizationId, long vkUserId, CancellationToken cancellationToken = default);
    Task LockForPendingLimitAsync(Guid clientId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Client>> GetAllWithContactsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Client>> FindWithContactsAsync(
        Expression<Func<Client, bool>> predicate,
        CancellationToken cancellationToken = default);
}
