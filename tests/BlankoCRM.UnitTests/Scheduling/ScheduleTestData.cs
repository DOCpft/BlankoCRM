using BlankoCRM.Application.DTOs;
using BlankoCRM.Core.Entities;

namespace BlankoCRM.UnitTests.Scheduling;

internal static class ScheduleTestData
{
    public static DateTime At(int hour) => new(2032, 2, 3, hour, 0, 0, DateTimeKind.Utc);

    public static CreateFreeSlotRequest FreeSlot(int startHour, int endHour, Guid? salonId = null) =>
        new(At(startHour), At(endHour), salonId);

    public static Schedule Slot(int startHour, int endHour, Guid? salonId = null) =>
        new()
        {
            Id = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid(),
            SalonId = salonId,
            StartTime = At(startHour),
            EndTime = At(endHour)
        };
}
