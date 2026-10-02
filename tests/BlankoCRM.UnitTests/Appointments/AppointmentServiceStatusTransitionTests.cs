using BlankoCRM.Core.Entities;
using BlankoCRM.Core.Exceptions;
using Moq;

namespace BlankoCRM.UnitTests.Appointments;

public sealed class AppointmentServiceStatusTransitionTests
{
    [Theory]
    [InlineData(AppointmentStatus.Pending, AppointmentTransition.Reject, AppointmentStatus.Rejected)]
    [InlineData(AppointmentStatus.Pending, AppointmentTransition.Cancel, AppointmentStatus.Cancelled)]
    [InlineData(AppointmentStatus.Scheduled, AppointmentTransition.Cancel, AppointmentStatus.Cancelled)]
    [InlineData(AppointmentStatus.Scheduled, AppointmentTransition.Complete, AppointmentStatus.Completed)]
    [InlineData(AppointmentStatus.Scheduled, AppointmentTransition.MarkNoShow, AppointmentStatus.NoShow)]
    public async Task Allowed_transitions_update_the_status(
        AppointmentStatus initialStatus,
        AppointmentTransition transition,
        AppointmentStatus expectedStatus)
    {
        var context = new AppointmentServiceTestContext();
        var appointment = AppointmentTestData.Appointment(initialStatus);
        context.AppointmentRepository
            .Setup(repository => repository.GetByIdWithDetailsAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        await ApplyTransitionAsync(context, appointment.Id, transition);

        Assert.Equal(expectedStatus, appointment.Status);
        context.UnitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelAsync_releases_the_schedule_for_a_scheduled_appointment()
    {
        var context = new AppointmentServiceTestContext();
        var appointment = AppointmentTestData.Appointment(AppointmentStatus.Scheduled);
        var schedule = new Schedule { Id = Guid.NewGuid(), AppointmentId = appointment.Id, Appointment = appointment };
        context.AppointmentRepository
            .Setup(repository => repository.GetByIdWithDetailsAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        context.ScheduleRepository
            .Setup(repository => repository.GetSlotByAppointmentIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);

        await context.Service.CancelAsync(appointment.Id);

        Assert.Equal(AppointmentStatus.Cancelled, appointment.Status);
        Assert.Null(schedule.AppointmentId);
        Assert.Null(schedule.Appointment);
    }

    [Theory]
    [InlineData(AppointmentStatus.Pending, AppointmentTransition.Complete)]
    [InlineData(AppointmentStatus.Completed, AppointmentTransition.Cancel)]
    [InlineData(AppointmentStatus.Scheduled, AppointmentTransition.Reject)]
    public async Task Disallowed_transitions_throw_a_conflict(
        AppointmentStatus initialStatus,
        AppointmentTransition transition)
    {
        var context = new AppointmentServiceTestContext();
        var appointment = AppointmentTestData.Appointment(initialStatus);
        context.AppointmentRepository
            .Setup(repository => repository.GetByIdWithDetailsAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        await Assert.ThrowsAsync<ConflictException>(
            () => ApplyTransitionAsync(context, appointment.Id, transition));

        context.UnitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    private static Task ApplyTransitionAsync(
        AppointmentServiceTestContext context,
        Guid appointmentId,
        AppointmentTransition transition) => transition switch
    {
        AppointmentTransition.Reject => context.Service.RejectAsync(appointmentId),
        AppointmentTransition.Cancel => context.Service.CancelAsync(appointmentId),
        AppointmentTransition.Complete => context.Service.CompleteAsync(appointmentId),
        AppointmentTransition.MarkNoShow => context.Service.MarkNoShowAsync(appointmentId),
        _ => throw new ArgumentOutOfRangeException(nameof(transition), transition, null)
    };

    public enum AppointmentTransition
    {
        Reject,
        Cancel,
        Complete,
        MarkNoShow
    }
}
