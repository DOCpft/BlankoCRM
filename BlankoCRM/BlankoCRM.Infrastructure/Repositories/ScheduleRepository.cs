using BlankoCRM.Core.Entities;
using BlankoCRM.Core.Interfaces;
using BlankoCRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BlankoCRM.Infrastructure.Repositories;

/// <summary>
/// EF Core репозиторий слотов расписания.
/// </summary>
public sealed class ScheduleRepository : EfRepository<Schedule>, IScheduleRepository
{
    public ScheduleRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Schedule?> GetByIdWithAppointmentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbContext.Schedules
            .Include(schedule => schedule.Appointment)
            .ThenInclude(appointment => appointment!.AppointmentServices)
            .FirstOrDefaultAsync(schedule => schedule.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Schedule>> GetByPeriodAsync(
        DateTime dateFrom,
        DateTime dateTo,
        CancellationToken cancellationToken = default)
    {
        return await DbContext.Schedules
            .AsNoTracking()
            .Include(schedule => schedule.Appointment)
            .ThenInclude(appointment => appointment!.AppointmentServices)
            .Where(schedule => schedule.SlotTime >= dateFrom && schedule.SlotTime <= dateTo)
            .OrderBy(schedule => schedule.SlotTime)
            .ToListAsync(cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<Schedule> entities, CancellationToken cancellationToken = default)
    {
        await using var transaction = await DbContext.Database.BeginTransactionAsync(cancellationToken);
        await DbContext.Schedules.AddRangeAsync(entities, cancellationToken);
        await DbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
