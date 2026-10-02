using BlankoCRM.Core.Entities;
using BlankoCRM.Core.Interfaces;
using BlankoCRM.Infrastructure.Data;
using BlankoCRM.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BlankoCRM.IntegrationTests.Appointments;

internal sealed class AppointmentTenantIsolationContext : IDisposable
{
    private readonly string _databaseName = Guid.NewGuid().ToString();

    public Guid CurrentOrganizationId { get; } = Guid.NewGuid();
    public Guid OtherOrganizationId { get; } = Guid.NewGuid();
    public Guid CurrentAppointmentId { get; } = Guid.NewGuid();
    public Guid OtherAppointmentId { get; } = Guid.NewGuid();

    public async Task SeedAsync()
    {
        using var context = CreateDbContext(null);
        await context.Database.EnsureCreatedAsync();
        var currentClientId = Guid.NewGuid();
        var otherClientId = Guid.NewGuid();
        context.Organizations.AddRange(
            new Organization
            {
                Id = CurrentOrganizationId,
                Name = "Current organization",
                Slug = $"current-{CurrentOrganizationId:N}",
                CreatedAt = DateTime.UtcNow
            },
            new Organization
            {
                Id = OtherOrganizationId,
                Name = "Other organization",
                Slug = $"other-{OtherOrganizationId:N}",
                CreatedAt = DateTime.UtcNow
            });
        context.Clients.AddRange(
            new Client
            {
                Id = currentClientId,
                OrganizationId = CurrentOrganizationId,
                Name = "Current client",
                CreatedAt = DateTime.UtcNow
            },
            new Client
            {
                Id = otherClientId,
                OrganizationId = OtherOrganizationId,
                Name = "Other client",
                CreatedAt = DateTime.UtcNow
            });
        context.Appointments.AddRange(
            CreateAppointment(CurrentAppointmentId, CurrentOrganizationId, currentClientId, "Current appointment"),
            CreateAppointment(OtherAppointmentId, OtherOrganizationId, otherClientId, "Other appointment"));
        await context.SaveChangesAsync();
    }

    public AppDbContext CreateDbContext(Guid? organizationId)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(_databaseName)
            .Options;
        return new AppDbContext(options, new TestOrganizationContext(organizationId));
    }

    public void Dispose()
    {
        using var context = CreateDbContext(null);
        context.Database.EnsureDeleted();
    }

    private static Appointment CreateAppointment(Guid appointmentId, Guid organizationId, Guid clientId, string notes)
    {
        return new Appointment
        {
            Id = appointmentId,
            OrganizationId = organizationId,
            ClientId = clientId,
            StartTime = new DateTime(2030, 1, 10, 10, 0, 0, DateTimeKind.Utc),
            EndTime = new DateTime(2030, 1, 10, 11, 0, 0, DateTimeKind.Utc),
            Status = AppointmentStatus.Scheduled,
            Notes = notes,
            CreatedAt = DateTime.UtcNow,
            AppointmentServices = []
        };
    }

    private sealed class TestOrganizationContext(Guid? organizationId) : IOrganizationContext
    {
        public Guid? GetCurrentOrganizationId() => organizationId;

        public void SetCurrentOrganizationId(Guid value) =>
            throw new NotSupportedException();

        public void Clear() =>
            throw new NotSupportedException();
    }
}
