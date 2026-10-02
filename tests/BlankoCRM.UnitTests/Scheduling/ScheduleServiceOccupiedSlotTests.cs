using BlankoCRM.Application.DTOs;
using BlankoCRM.Core.Entities;
using Moq;

namespace BlankoCRM.UnitTests.Scheduling;

public sealed class ScheduleServiceOccupiedSlotTests
{
    [Fact]
    public async Task AddOccupiedSlotAsync_delegates_creation_to_the_appointment_service()
    {
        var context = new ScheduleServiceTestContext();
        var appointment = new Appointment { Id = Guid.NewGuid() };
        var schedule = ScheduleTestData.Slot(9, 10);
        var services = new List<ServiceInput> { new(Guid.NewGuid()) };
        var request = new CreateOccupiedSlotRequest(
            ScheduleTestData.At(9),
            new CreateOccupiedSlotAppointmentData(Guid.NewGuid(), services, "notes"),
            ScheduleTestData.At(10));
        context.AppointmentService
            .Setup(service => service.CreateWithScheduleAsync(
                request.AppointmentData.ClientId,
                services,
                request.StartTime,
                request.AppointmentData.Notes,
                request.AppointmentData.SalonId,
                request.EndTime,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((appointment, schedule));

        var result = await context.Service.AddOccupiedSlotAsync(request);

        Assert.Same(schedule, result);
        context.AppointmentService.Verify(
            service => service.CreateWithScheduleAsync(
                request.AppointmentData.ClientId,
                services,
                request.StartTime,
                request.AppointmentData.Notes,
                request.AppointmentData.SalonId,
                request.EndTime,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
