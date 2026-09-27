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
            .Include(schedule => schedule.Appointment)
                .ThenInclude(client => client!.Client)
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
            .Include(schedule => schedule.Appointment)
                .ThenInclude(client => client!.Client)
            .Where(schedule => schedule.StartTime <= dateTo && schedule.EndTime > dateFrom)
            .OrderBy(schedule => schedule.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<Schedule?> GetSlotByAppointmentIdAsync(Guid appointmentId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Schedules
            .Include(schedule => schedule.Appointment)
            .FirstOrDefaultAsync(schedule => schedule.AppointmentId == appointmentId, cancellationToken);
    }

    public Task<bool> HasOverlapWithFreeSlotsAsync(
        DateTime startTime,
        DateTime endTime,
        Guid organizationId,
        Guid? salonId = null,
        Guid? excludedSlotId = null,
        CancellationToken cancellationToken = default)
    {
        return DbContext.Schedules.AnyAsync(schedule =>
            schedule.OrganizationId == organizationId
            && schedule.SalonId == salonId
            && schedule.AppointmentId == null
            && (!excludedSlotId.HasValue || schedule.Id != excludedSlotId.Value)
            && schedule.StartTime < endTime
            && schedule.EndTime > startTime,
            cancellationToken);
    }

    public async Task<int> DeleteFreeSlotsByTimeAsync(
        DateTime startTime,
        DateTime endTime,
        Guid organizationId,
        Guid? salonId = null,
        Guid? excludedSlotId = null,
        CancellationToken cancellationToken = default)
    {
        var slots = await DbContext.Schedules
            .Where(schedule =>
                schedule.OrganizationId == organizationId
                && schedule.SalonId == salonId
                && schedule.AppointmentId == null
                && (!excludedSlotId.HasValue || schedule.Id != excludedSlotId.Value)
                && schedule.StartTime < endTime
                && schedule.EndTime > startTime)
            .ToListAsync(cancellationToken);

        DbContext.Schedules.RemoveRange(slots);
        return slots.Count;
    }

    public async Task AddRangeAsync(IEnumerable<Schedule> entities, CancellationToken cancellationToken = default)
    {
        await DbContext.Schedules.AddRangeAsync(entities, cancellationToken);
    }
}
