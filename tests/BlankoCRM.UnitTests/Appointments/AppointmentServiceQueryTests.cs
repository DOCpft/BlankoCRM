using BlankoCRM.Core.Entities;
using Moq;

namespace BlankoCRM.UnitTests.Appointments;

public sealed class AppointmentServiceQueryTests
{
    [Fact]
    public async Task GetByIdAsync_returns_the_appointment_or_null()
    {
        var context = new AppointmentServiceTestContext();
        var appointment = AppointmentTestData.Appointment();
        context.AppointmentRepository
            .Setup(repository => repository.GetByIdWithDetailsAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        var result = await context.Service.GetByIdAsync(appointment.Id);

        Assert.Same(appointment, result);
    }

    [Fact]
    public async Task GetByIdAsync_returns_null_when_the_appointment_does_not_exist()
    {
        var context = new AppointmentServiceTestContext();
        var appointmentId = Guid.NewGuid();
        context.AppointmentRepository
            .Setup(repository => repository.GetByIdWithDetailsAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        var result = await context.Service.GetByIdAsync(appointmentId);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAppointmentsAsync_returns_the_organization_appointments()
    {
        var context = new AppointmentServiceTestContext();
        List<Appointment> appointments = [AppointmentTestData.Appointment(), AppointmentTestData.Appointment()];
        context.AppointmentRepository
            .Setup(repository => repository.GetAllWithDetailsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointments);

        var result = await context.Service.GetAllAppointmentsAsync();

        Assert.Equal(appointments, result);
    }

    [Fact]
    public async Task GetForDayAsync_returns_appointments_for_the_requested_day()
    {
        var context = new AppointmentServiceTestContext();
        var date = AppointmentTestData.At(0);
        List<Appointment> appointments = [AppointmentTestData.Appointment()];
        context.AppointmentRepository
            .Setup(repository => repository.GetForDayAsync(date, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointments);

        var result = await context.Service.GetForDayAsync(date);

        Assert.Equal(appointments, result);
    }

    [Fact]
    public async Task GetPendingAppointmentsAsync_returns_active_pending_appointments_for_the_current_organization()
    {
        var context = new AppointmentServiceTestContext();
        List<Appointment> appointments = [AppointmentTestData.Appointment(AppointmentStatus.Pending)];
        context.AppointmentRepository
            .Setup(repository => repository.GetActivePendingAsync(
                context.OrganizationId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointments);

        var result = await context.Service.GetPendingAppointmentsAsync();

        Assert.Equal(appointments, result);
        context.AppointmentRepository.Verify(repository => repository.GetActivePendingAsync(
            context.OrganizationId, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPendingAppointmentsAsync_requires_an_organization_context()
    {
        var context = new AppointmentServiceTestContext();
        context.OrganizationContext
            .Setup(organizationContext => organizationContext.GetCurrentOrganizationId())
            .Returns((Guid?)null);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => context.Service.GetPendingAppointmentsAsync());

        context.AppointmentRepository.Verify(repository => repository.GetActivePendingAsync(
            It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
