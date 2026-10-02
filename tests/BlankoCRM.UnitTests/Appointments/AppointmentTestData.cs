using BlankoCRM.Core.Entities;

namespace BlankoCRM.UnitTests.Appointments;

internal static class AppointmentTestData
{
    public static DateTime At(int hour) => new(2032, 2, 3, hour, 0, 0, DateTimeKind.Utc);

    public static Appointment Appointment(AppointmentStatus status = AppointmentStatus.Scheduled) => new()
    {
        Id = Guid.NewGuid(),
        OrganizationId = Guid.NewGuid(),
        ClientId = Guid.NewGuid(),
        StartTime = At(9),
        EndTime = At(10),
        Status = status,
        AppointmentServices = []
    };

    public static Client Client() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Test client"
    };

    public static Service Service(int durationMinutes = 30, decimal price = 50m) => new()
    {
        Id = Guid.NewGuid(),
        Name = "Test service",
        DurationMinutes = durationMinutes,
        Price = price
    };
}
