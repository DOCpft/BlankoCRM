using BlankoCRM.Core.Entities;
using BlankoCRM.Core.Exceptions;
using Moq;

namespace BlankoCRM.UnitTests.PublicAppointments;

public sealed class PublicAppointmentServiceLifecycleTests
{
    [Fact]
    public async Task WithdrawAsync_withdraws_the_clients_active_pending_request()
    {
        var scenario = CreateOwnedAppointment(AppointmentStatus.Pending, expiresInHours: 12);

        await scenario.Context.Service.WithdrawAsync(scenario.Appointment.Id, scenario.Context.VkParams);

        Assert.Equal(AppointmentStatus.Withdrawn, scenario.Appointment.Status);
        scenario.Context.UnitOfWork.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        scenario.Context.Transaction.Verify(
            transaction => transaction.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task WithdrawAsync_returns_not_found_for_an_appointment_owned_by_another_client()
    {
        var scenario = CreateOwnedAppointment(AppointmentStatus.Pending, expiresInHours: 12);
        scenario.Appointment.ClientId = Guid.NewGuid();

        await Assert.ThrowsAsync<NotFoundException>(() =>
            scenario.Context.Service.WithdrawAsync(scenario.Appointment.Id, scenario.Context.VkParams));

        scenario.Context.UnitOfWork.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WithdrawAsync_rejects_an_expired_request()
    {
        var scenario = CreateOwnedAppointment(AppointmentStatus.Pending, expiresInHours: -1);

        await Assert.ThrowsAsync<ConflictException>(() =>
            scenario.Context.Service.WithdrawAsync(scenario.Appointment.Id, scenario.Context.VkParams));

        Assert.Equal(AppointmentStatus.Pending, scenario.Appointment.Status);
    }

    [Fact]
    public async Task WithdrawAsync_rejects_a_request_that_is_not_pending()
    {
        var scenario = CreateOwnedAppointment(AppointmentStatus.Scheduled);

        await Assert.ThrowsAsync<ConflictException>(() =>
            scenario.Context.Service.WithdrawAsync(scenario.Appointment.Id, scenario.Context.VkParams));

        Assert.Equal(AppointmentStatus.Scheduled, scenario.Appointment.Status);
    }

    [Fact]
    public async Task CancelAsync_changes_a_pending_request_to_withdrawn()
    {
        var scenario = CreateOwnedAppointment(AppointmentStatus.Pending, expiresInHours: 12);

        await scenario.Context.Service.CancelAsync(scenario.Appointment.Id, scenario.Context.VkParams);

        Assert.Equal(AppointmentStatus.Withdrawn, scenario.Appointment.Status);
        scenario.Context.ScheduleRepository.Verify(repository => repository.GetSlotByAppointmentIdAsync(
            It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CancelAsync_cancels_a_scheduled_request_and_releases_its_slot()
    {
        var scenario = CreateOwnedAppointment(AppointmentStatus.Scheduled);
        var schedule = new Schedule
        {
            Id = Guid.NewGuid(),
            AppointmentId = scenario.Appointment.Id,
            Appointment = scenario.Appointment
        };
        scenario.Context.ScheduleRepository
            .Setup(repository => repository.GetSlotByAppointmentIdAsync(
                scenario.Appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);

        await scenario.Context.Service.CancelAsync(scenario.Appointment.Id, scenario.Context.VkParams);

        Assert.Equal(AppointmentStatus.Cancelled, scenario.Appointment.Status);
        Assert.Null(schedule.AppointmentId);
        Assert.Null(schedule.Appointment);
    }

    [Fact]
    public async Task CancelAsync_rejects_a_request_in_a_terminal_status()
    {
        var scenario = CreateOwnedAppointment(AppointmentStatus.Completed);

        await Assert.ThrowsAsync<ConflictException>(() =>
            scenario.Context.Service.CancelAsync(scenario.Appointment.Id, scenario.Context.VkParams));

        Assert.Equal(AppointmentStatus.Completed, scenario.Appointment.Status);
    }

    private static (PublicAppointmentTestContext Context, Appointment Appointment) CreateOwnedAppointment(
        AppointmentStatus status,
        int? expiresInHours = null)
    {
        var context = new PublicAppointmentTestContext();
        var client = PublicAppointmentTestData.CreateClient();
        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            Status = status,
            AppointmentServices = []
        };
        appointment.OrganizationId = context.OrganizationId;
        appointment.ClientId = client.Id;
        if (expiresInHours.HasValue)
        {
            appointment.ExpiresAt = DateTime.UtcNow.AddHours(expiresInHours.Value);
        }

        context.ClientRepository
            .Setup(repository => repository.FindByVkUserIdAsync(
                context.OrganizationId,
                PublicAppointmentTestContext.VkUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);
        context.AppointmentRepository
            .Setup(repository => repository.GetByIdWithDetailsAsync(
                appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        return (context, appointment);
    }
}
