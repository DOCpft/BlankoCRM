using BlankoCRM.Core.Entities;
using BlankoCRM.Core.Exceptions;
using Moq;

namespace BlankoCRM.UnitTests.Scheduling;

public sealed class ScheduleServiceUpdateTests
{
    [Fact]
    public async Task UpdateSlotAsync_updates_a_free_slots_time_range()
    {
        var context = new ScheduleServiceTestContext();
        var slot = ScheduleTestData.Slot(9, 10);
        context.ScheduleRepository
            .Setup(repository => repository.GetByIdWithAppointmentAsync(slot.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(slot);

        await context.Service.UpdateSlotAsync(
            slot.Id,
            new BlankoCRM.Application.DTOs.UpdateSlotRequest(ScheduleTestData.At(10), ScheduleTestData.At(11)));

        Assert.Equal(ScheduleTestData.At(10), slot.StartTime);
        Assert.Equal(ScheduleTestData.At(11), slot.EndTime);
        context.UnitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        context.Transaction.Verify(transaction => transaction.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateSlotAsync_rejects_a_conflicting_free_slot_time()
    {
        var context = new ScheduleServiceTestContext();
        var slot = ScheduleTestData.Slot(9, 10);
        context.ScheduleRepository
            .Setup(repository => repository.GetByIdWithAppointmentAsync(slot.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(slot);
        context.ScheduleRepository
            .Setup(repository => repository.HasOverlapWithFreeSlotsAsync(
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), context.OrganizationId,
                slot.SalonId, slot.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() => context.Service.UpdateSlotAsync(
            slot.Id,
            new BlankoCRM.Application.DTOs.UpdateSlotRequest(ScheduleTestData.At(10), ScheduleTestData.At(11))));

        Assert.Equal(ScheduleTestData.At(9), slot.StartTime);
        context.UnitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateSlotAsync_rejects_a_time_conflicting_with_another_appointment()
    {
        var context = new ScheduleServiceTestContext();
        var appointment = new Appointment { Id = Guid.NewGuid(), SalonId = Guid.NewGuid() };
        var slot = ScheduleTestData.Slot(9, 10);
        slot.Appointment = appointment;
        slot.AppointmentId = appointment.Id;
        context.ScheduleRepository
            .Setup(repository => repository.GetByIdWithAppointmentAsync(slot.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(slot);
        context.AppointmentRepository
            .Setup(repository => repository.HasOverlapAsync(
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), context.OrganizationId,
                appointment.SalonId, appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() => context.Service.UpdateSlotAsync(
            slot.Id,
            new BlankoCRM.Application.DTOs.UpdateSlotRequest(ScheduleTestData.At(10), ScheduleTestData.At(11))));

        context.UnitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateSlotAsync_does_not_allow_attaching_an_appointment_to_a_free_slot()
    {
        var context = new ScheduleServiceTestContext();
        var slot = ScheduleTestData.Slot(9, 10);
        context.ScheduleRepository
            .Setup(repository => repository.GetByIdWithAppointmentAsync(slot.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(slot);

        await Assert.ThrowsAsync<ValidationException>(() => context.Service.UpdateSlotAsync(
            slot.Id,
            new BlankoCRM.Application.DTOs.UpdateSlotRequest(
                null,
                null,
                new BlankoCRM.Application.DTOs.UpdateSlotAppointmentData(null, null, null))));

        context.AppointmentService.Verify(
            service => service.UpdateAsync(
                It.IsAny<Guid>(),
                It.IsAny<BlankoCRM.Application.DTOs.UpdateAppointmentRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        context.UnitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
