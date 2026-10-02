using BlankoCRM.Core.Entities;

namespace BlankoCRM.IntegrationTests.Appointments;

public sealed class AppointmentTenantIsolationTests
{
    [Fact]
    public async Task GetByIdWithDetailsAsync_does_not_return_another_organizations_appointment()
    {
        using var context = new AppointmentTenantIsolationContext();
        await context.SeedAsync();
        using var database = context.CreateDbContext(context.CurrentOrganizationId);
        var repository = new Infrastructure.Repositories.AppointmentRepository(database);

        var currentAppointment = await repository.GetByIdWithDetailsAsync(context.CurrentAppointmentId);
        var otherAppointment = await repository.GetByIdWithDetailsAsync(context.OtherAppointmentId);

        Assert.NotNull(currentAppointment);
        Assert.Equal(context.CurrentOrganizationId, currentAppointment.OrganizationId);
        Assert.Null(otherAppointment);
    }

    [Fact]
    public async Task GetAllWithDetailsAsync_returns_only_current_organizations_appointments()
    {
        using var context = new AppointmentTenantIsolationContext();
        await context.SeedAsync();
        using var database = context.CreateDbContext(context.CurrentOrganizationId);
        var repository = new Infrastructure.Repositories.AppointmentRepository(database);

        var appointments = await repository.GetAllWithDetailsAsync();

        var appointment = Assert.Single(appointments);
        Assert.Equal(context.CurrentAppointmentId, appointment.Id);
        Assert.Equal(context.CurrentOrganizationId, appointment.OrganizationId);
    }

    [Fact]
    public async Task GetForDayAsync_returns_only_current_organizations_appointments()
    {
        using var context = new AppointmentTenantIsolationContext();
        await context.SeedAsync();
        using var database = context.CreateDbContext(context.CurrentOrganizationId);
        var repository = new Infrastructure.Repositories.AppointmentRepository(database);

        var appointments = await repository.GetForDayAsync(new DateTime(2030, 1, 10));

        var appointment = Assert.Single(appointments);
        Assert.Equal(context.CurrentAppointmentId, appointment.Id);
        Assert.Equal(context.CurrentOrganizationId, appointment.OrganizationId);
    }

    [Fact]
    public async Task FindAsync_does_not_load_another_organizations_appointment_for_update()
    {
        using var context = new AppointmentTenantIsolationContext();
        await context.SeedAsync();
        using (var tenantContext = context.CreateDbContext(context.CurrentOrganizationId))
        {
            var otherAppointment = await tenantContext.Appointments.FindAsync(context.OtherAppointmentId);

            Assert.Null(otherAppointment);
        }

        using (var verificationContext = context.CreateDbContext(context.OtherOrganizationId))
        {
            var otherAppointment = await verificationContext.Appointments.FindAsync(context.OtherAppointmentId);

            Assert.NotNull(otherAppointment);
            Assert.Equal("Other appointment", otherAppointment.Notes);
        }
    }

}
