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

    public async Task<IReadOnlyList<Appointment>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default)
    {
        return await DbContext.Appointments
            .AsNoTracking()
            .Include(appointment => appointment.Client)
            .Include(appointment => appointment.AppointmentServices)
            .OrderBy(appointment => appointment.StartTime)
            .ToListAsync(cancellationToken);
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
                && (salonId == null || appointment.SalonId == salonId)
                && appointment.StartTime < endTime
                && appointment.EndTime > startTime);

        if (excludedAppointmentId.HasValue)
        {
            query = query.Where(appointment => appointment.Id != excludedAppointmentId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }
}
