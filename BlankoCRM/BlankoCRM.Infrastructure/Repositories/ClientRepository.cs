using System.Linq.Expressions;
using BlankoCRM.Core.Entities;
using BlankoCRM.Core.Interfaces;
using BlankoCRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BlankoCRM.Infrastructure.Repositories;

public sealed class ClientRepository : EfRepository<Client>, IClientRepository
{
    public ClientRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public Task<Client?> GetByIdWithContactsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return DbContext.Clients
            .Include(client => client.Contacts)
            .FirstOrDefaultAsync(client => client.Id == id, cancellationToken);
    }

    public Task<Client?> FindByPhoneAsync(
        Guid organizationId,
        string normalizedPhone,
        CancellationToken cancellationToken = default)
    {
        return DbContext.Clients
            .AsTracking()
            .Include(client => client.Contacts)
            .FirstOrDefaultAsync(
                client => client.OrganizationId == organizationId && client.Phone == normalizedPhone,
                cancellationToken);
    }

    public Task<Client?> FindByVkUserIdAsync(
        Guid organizationId,
        long vkUserId,
        CancellationToken cancellationToken = default)
    {
        var contactValue = vkUserId.ToString(System.Globalization.CultureInfo.InvariantCulture);
        return DbContext.Clients
            .AsTracking()
            .Include(client => client.Contacts)
            .FirstOrDefaultAsync(
                client => client.OrganizationId == organizationId
                    && client.Contacts.Any(contact => contact.OrganizationId == organizationId
                        && contact.Type == ContactType.Vk
                        && contact.Value == contactValue),
                cancellationToken);
    }

    public async Task LockForPendingLimitAsync(
        Guid clientId,
        CancellationToken cancellationToken = default)
    {
        await DbContext.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock(hashtextextended({clientId.ToString()}::text, 0))",
            cancellationToken);
    }

    public async Task<IReadOnlyList<Client>> GetAllWithContactsAsync(CancellationToken cancellationToken = default)
    {
        return await DbContext.Clients
            .AsNoTracking()
            .Include(client => client.Contacts)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Client>> FindWithContactsAsync(
        Expression<Func<Client, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await DbContext.Clients
            .Where(predicate)
            .AsNoTracking()
            .Include(client => client.Contacts)
            .ToListAsync(cancellationToken);
    }
}
