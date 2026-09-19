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
