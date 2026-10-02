using BlankoCRM.Core.Entities;
using BlankoCRM.Core.Interfaces;
using BlankoCRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BlankoCRM.Infrastructure.Repositories;

/// <summary>
/// EF Core репозиторий настроек напоминаний мастера.
/// </summary>
public sealed class MasterReminderSettingRepository : EfRepository<MasterReminderSetting>, IMasterReminderSettingRepository
{
    public MasterReminderSettingRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<IReadOnlyList<MasterReminderSetting>> GetByAppUserIdAsync(
        Guid appUserId,
        CancellationToken cancellationToken = default)
    {
        return await DbContext.Set<MasterReminderSetting>()
            .IgnoreQueryFilters()
            .Where(setting => setting.AppUserId == appUserId)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> IsEnabledAsync(
        Guid appUserId,
        int offsetHours,
        CancellationToken cancellationToken = default)
    {
        return DbContext.Set<MasterReminderSetting>()
            .IgnoreQueryFilters()
            .AnyAsync(
                setting => setting.AppUserId == appUserId
                    && setting.OffsetHours == offsetHours
                    && setting.Enabled,
                cancellationToken);
    }
}
