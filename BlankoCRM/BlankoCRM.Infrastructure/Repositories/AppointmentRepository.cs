using BlankoCRM.Core.Entities;
using BlankoCRM.Core.Interfaces;
using BlankoCRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BlankoCRM.Infrastructure.Repositories;

/// <summary>
/// EF Core репозиторий записей.
/// </summary>
public sealed class AppointmentRepository : EfRepository<Appointment>, IAppointmentRepository
{
    public AppointmentRepository(AppDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<Appointment?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbContext.Appointments
            .Include(appointment => appointment.Client)
            .Include(appointment => appointment.AppointmentServices)
            .FirstOrDefaultAsync(appointment => appointment.Id == id, cancellationToken);
    }

    public async Task LockForUpdateAsync(
        Guid organizationId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await DbContext.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT 1 FROM \"Appointments\" WHERE \"OrganizationId\" = {organizationId} AND \"Id\" = {id} FOR UPDATE",
            cancellationToken);
    }

    public async Task<IReadOnlyList<Appointment>> GetForDayAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        var dayStart = date.Date;
        var dayEnd = dayStart.AddDays(1);

        return await DbContext.Appointments
            .AsNoTracking()
            .Include(appointment => appointment.Client)
            .Include(appointment => appointment.AppointmentServices)
            .Where(appointment => appointment.StartTime >= dayStart && appointment.StartTime < dayEnd)
            .OrderBy(appointment => appointment.StartTime)
            .ToListAsync(cancellationToken);
    }

    public Task<int> CountActivePendingAsync(
        Guid organizationId,
        Guid clientId,
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        return DbContext.Appointments.CountAsync(appointment =>
            appointment.OrganizationId == organizationId
            && appointment.ClientId == clientId
            && appointment.Status == AppointmentStatus.Pending
            && appointment.ExpiresAt.HasValue
            && appointment.ExpiresAt.Value > now,
            cancellationToken);
    }

    public async Task<IReadOnlyList<Appointment>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default)
    {
        return await DbContext.Appointments
            .AsNoTracking()
            .Include(appointment => appointment.Client)
            .Include(appointment => appointment.AppointmentServices)
            .OrderBy(appointment => appointment.StartTime)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Appointment>> GetActivePendingAsync(
        Guid organizationId,
        DateTime now,
        CancellationToken cancellationToken = default)
    {
        return await DbContext.Appointments
            .AsNoTracking()
            .Include(appointment => appointment.Client)
            .Include(appointment => appointment.AppointmentServices)
            .Where(appointment => appointment.OrganizationId == organizationId
                && appointment.Status == AppointmentStatus.Pending
                && appointment.ExpiresAt.HasValue
                && appointment.ExpiresAt.Value > now)
            .OrderBy(appointment => appointment.StartTime)
            .ThenBy(appointment => appointment.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CancelExpiredPendingBatchAsync(
        DateTime now,
        int batchSize,
        CancellationToken cancellationToken = default)
    {
        var expiredIds = await DbContext.Appointments
            .IgnoreQueryFilters()
            .Where(appointment => appointment.Status == AppointmentStatus.Pending
                && appointment.ExpiresAt <= now)
            .OrderBy(appointment => appointment.ExpiresAt)
            .Select(appointment => appointment.Id)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        if (expiredIds.Count == 0)
        {
            return 0;
        }

        return await DbContext.Appointments
            .IgnoreQueryFilters()
            .Where(appointment => expiredIds.Contains(appointment.Id)
                && appointment.Status == AppointmentStatus.Pending
                && appointment.ExpiresAt <= now)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(appointment => appointment.Status, AppointmentStatus.Cancelled)
                .SetProperty(appointment => appointment.UpdatedAt, now),
                cancellationToken);
    }

    public Task<bool> HasOverlapAsync(
        DateTime startTime,
        DateTime endTime,
        Guid organizationId,
        Guid? salonId = null,
        Guid? excludedAppointmentId = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbContext.Appointments
            .Where(appointment => appointment.OrganizationId == organizationId
                && appointment.SalonId == salonId
                && (appointment.Status == AppointmentStatus.Scheduled
                    || appointment.Status == AppointmentStatus.Completed
                    || appointment.Status == AppointmentStatus.NoShow)
                && appointment.StartTime < endTime
                && appointment.EndTime > startTime);

        if (excludedAppointmentId.HasValue)
        {
            query = query.Where(appointment => appointment.Id != excludedAppointmentId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }
}
