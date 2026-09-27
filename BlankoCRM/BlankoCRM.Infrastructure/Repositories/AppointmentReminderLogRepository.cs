using BlankoCRM.Core.Entities;
using BlankoCRM.Core.Interfaces;
using BlankoCRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BlankoCRM.Infrastructure.Repositories;

/// <summary>
/// EF Core репозиторий журнала отправленных напоминаний.
/// </summary>
public sealed class AppointmentReminderLogRepository : EfRepository<AppointmentReminderLog>, IAppointmentReminderLogRepository
{
    public AppointmentReminderLogRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public Task<bool> ExistsAsync(
        Guid appointmentId,
        int offsetHours,
        CancellationToken cancellationToken = default)
    {
        return DbContext.Set<AppointmentReminderLog>()
            .IgnoreQueryFilters()
            .AnyAsync(
                log => log.AppointmentId == appointmentId && log.OffsetHours == offsetHours,
                cancellationToken);
    }
}
