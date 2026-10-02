using BlankoCRM.Core.Entities;
using BlankoCRM.Core.Exceptions;
using Moq;

namespace BlankoCRM.UnitTests.Scheduling;

public sealed class ScheduleServiceDeleteTests
{
    [Fact]
    public async Task RemoveSlotAsync_removes_a_free_slot()
    {
        var context = new ScheduleServiceTestContext();
        var slot = ScheduleTestData.Slot(9, 10);
        context.ScheduleRepository
            .Setup(repository => repository.GetByIdWithAppointmentAsync(slot.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(slot);

        var result = await context.Service.RemoveSlotAsync(slot.Id);

        Assert.True(result);
        context.UnitOfWork.Verify(unitOfWork => unitOfWork.Remove(slot), Times.Once);
        context.UnitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveSlotAsync_throws_when_the_slot_does_not_exist()
    {
        var context = new ScheduleServiceTestContext();
        var slotId = Guid.NewGuid();
        context.ScheduleRepository
            .Setup(repository => repository.GetByIdWithAppointmentAsync(slotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Schedule?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => context.Service.RemoveSlotAsync(slotId));

        context.UnitOfWork.Verify(unitOfWork => unitOfWork.Remove(It.IsAny<Schedule>()), Times.Never);
    }

    [Fact]
    public async Task RemoveSlotAsync_archives_a_linked_appointment_and_detaches_it()
    {
        var context = new ScheduleServiceTestContext();
        var appointment = new Appointment { Id = Guid.NewGuid(), Status = AppointmentStatus.Scheduled };
        var slot = ScheduleTestData.Slot(9, 10);
        slot.Appointment = appointment;
        slot.AppointmentId = appointment.Id;
        context.ScheduleRepository
            .Setup(repository => repository.GetByIdWithAppointmentAsync(slot.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(slot);

        await context.Service.RemoveSlotAsync(slot.Id);

        Assert.Equal(AppointmentStatus.Archived, appointment.Status);
        Assert.Null(slot.AppointmentId);
        Assert.Null(slot.Appointment);
    }

    [Fact]
    public async Task RemoveSlotsAsync_removes_existing_slots_and_archives_linked_appointments()
    {
        var context = new ScheduleServiceTestContext();
        var appointment = new Appointment { Id = Guid.NewGuid(), Status = AppointmentStatus.Scheduled };
        var first = ScheduleTestData.Slot(9, 10);
        var second = ScheduleTestData.Slot(11, 12);
        first.Appointment = appointment;
        first.AppointmentId = appointment.Id;
        var slots = new Dictionary<Guid, Schedule> { [first.Id] = first, [second.Id] = second };
        context.ScheduleRepository
            .Setup(repository => repository.GetByIdWithAppointmentAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => slots.GetValueOrDefault(id));

        var result = await context.Service.RemoveSlotsAsync([first.Id, second.Id]);

        Assert.Equal(2, result);
        Assert.Equal(AppointmentStatus.Archived, appointment.Status);
        Assert.Null(first.AppointmentId);
        Assert.Null(first.Appointment);
        context.UnitOfWork.Verify(unitOfWork => unitOfWork.Remove(first), Times.Once);
        context.UnitOfWork.Verify(unitOfWork => unitOfWork.Remove(second), Times.Once);
        context.UnitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveSlotsAsync_returns_zero_when_no_requested_slots_exist()
    {
        var context = new ScheduleServiceTestContext();
        context.ScheduleRepository
            .Setup(repository => repository.GetByIdWithAppointmentAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Schedule?)null);

        var result = await context.Service.RemoveSlotsAsync([Guid.NewGuid()]);

        Assert.Equal(0, result);
        context.UnitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RemoveSlotsAsync_returns_zero_for_an_empty_list()
    {
        var context = new ScheduleServiceTestContext();

        var result = await context.Service.RemoveSlotsAsync([]);

        Assert.Equal(0, result);
        context.ScheduleRepository.Verify(
            repository => repository.GetByIdWithAppointmentAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
