using BlankoCRM.Application.DTOs;
using BlankoCRM.Core.Entities;

namespace BlankoCRM.UnitTests.PublicAppointments;

internal static class PublicAppointmentTestData
{
    public static Service CreateService(int durationMinutes = 30, decimal price = 50m) => new()
    {
        Id = Guid.NewGuid(),
        Name = "Public service",
        DurationMinutes = durationMinutes,
        Price = price
    };

    public static Client CreateClient() => new()
    {
        Id = Guid.NewGuid(),
        Name = "Public client",
        Contacts = []
    };

    public static Schedule CreateFreeSlot(CreatePublicAppointmentRequest request, int extraMinutesAfter = 120) => new()
    {
        Id = request.FreeSlotId,
        StartTime = request.StartTime.UtcDateTime.AddMinutes(-30),
        EndTime = request.StartTime.UtcDateTime.AddMinutes(extraMinutesAfter)
    };
}
