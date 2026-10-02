using BlankoCRM.Core.Entities;
using BlankoCRM.Core.Exceptions;
using Moq;

namespace BlankoCRM.UnitTests.Appointments;

public sealed class AppointmentServiceConfirmationTests
{
    [Fact]
    public async Task ConfirmAsync_confirms_an_active_pending_appointment_and_creates_a_schedule()
    {
        var context = new AppointmentServiceTestContext();
        var appointment = AppointmentTestData.Appointment(AppointmentStatus.Pending);
        appointment.ExpiresAt = DateTime.UtcNow.AddHours(24);
        Schedule? addedSchedule = null;
        context.AppointmentRepository
            .Setup(repository => repository.GetByIdWithDetailsAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        context.ScheduleRepository
            .Setup(repository => repository.GetSlotByAppointmentIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Schedule?)null);
        context.UnitOfWork
            .Setup(unitOfWork => unitOfWork.Add(It.IsAny<Schedule>()))
            .Callback<Schedule>(schedule => addedSchedule = schedule);

        await context.Service.ConfirmAsync(appointment.Id);

        Assert.Equal(AppointmentStatus.Scheduled, appointment.Status);
        Assert.NotNull(addedSchedule);
        Assert.Equal(context.OrganizationId, addedSchedule.OrganizationId);
        Assert.Equal(appointment.Id, addedSchedule.AppointmentId);
        Assert.Same(appointment, addedSchedule.Appointment);
        Assert.Same(addedSchedule, appointment.Schedule);
        context.ScheduleRepository.Verify(repository => repository.DeleteFreeSlotsByTimeAsync(
            appointment.StartTime, appointment.EndTime, context.OrganizationId, appointment.SalonId,
            It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Once);
        context.Transaction.Verify(transaction => transaction.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ConfirmAsync_rejects_an_expired_pending_appointment()
    {
        var context = new AppointmentServiceTestContext();
        var appointment = AppointmentTestData.Appointment(AppointmentStatus.Pending);
        appointment.ExpiresAt = DateTime.UtcNow.AddHours(-1);
        context.AppointmentRepository
            .Setup(repository => repository.GetByIdWithDetailsAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        await Assert.ThrowsAsync<ConflictException>(() => context.Service.ConfirmAsync(appointment.Id));

        Assert.Equal(AppointmentStatus.Pending, appointment.Status);
        context.UnitOfWork.Verify(unitOfWork => unitOfWork.Add(It.IsAny<Schedule>()), Times.Never);
    }

    [Fact]
    public async Task ConfirmAsync_rejects_an_appointment_that_overlaps_an_existing_booking()
    {
        var context = new AppointmentServiceTestContext();
        var appointment = AppointmentTestData.Appointment(AppointmentStatus.Pending);
        appointment.ExpiresAt = DateTime.UtcNow.AddHours(24);
        context.AppointmentRepository
            .Setup(repository => repository.GetByIdWithDetailsAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        context.AppointmentRepository
            .Setup(repository => repository.HasOverlapAsync(
                appointment.StartTime, appointment.EndTime, context.OrganizationId,
                appointment.SalonId, appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() => context.Service.ConfirmAsync(appointment.Id));

        Assert.Equal(AppointmentStatus.Pending, appointment.Status);
        context.UnitOfWork.Verify(unitOfWork => unitOfWork.Add(It.IsAny<Schedule>()), Times.Never);
    }

    [Fact]
    public async Task ConfirmAsync_rejects_a_request_that_already_has_a_schedule()
    {
        var context = new AppointmentServiceTestContext();
        var appointment = AppointmentTestData.Appointment(AppointmentStatus.Pending);
        appointment.ExpiresAt = DateTime.UtcNow.AddHours(24);
        context.AppointmentRepository
            .Setup(repository => repository.GetByIdWithDetailsAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        context.ScheduleRepository
            .Setup(repository => repository.GetSlotByAppointmentIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Schedule { Id = Guid.NewGuid(), AppointmentId = appointment.Id });

        await Assert.ThrowsAsync<ConflictException>(() => context.Service.ConfirmAsync(appointment.Id));

        Assert.Equal(AppointmentStatus.Pending, appointment.Status);
        context.UnitOfWork.Verify(unitOfWork => unitOfWork.Add(It.IsAny<Schedule>()), Times.Never);
    }

    [Fact]
    public async Task ConfirmAsync_rejects_an_appointment_that_is_not_pending()
    {
        var context = new AppointmentServiceTestContext();
        var appointment = AppointmentTestData.Appointment(AppointmentStatus.Scheduled);
        context.AppointmentRepository
            .Setup(repository => repository.GetByIdWithDetailsAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        await Assert.ThrowsAsync<ConflictException>(() => context.Service.ConfirmAsync(appointment.Id));

        context.UnitOfWork.Verify(unitOfWork => unitOfWork.Add(It.IsAny<Schedule>()), Times.Never);
    }
}
