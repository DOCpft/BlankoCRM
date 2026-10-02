using BlankoCRM.Application.DTOs;
using BlankoCRM.Core.Entities;
using BlankoCRM.Core.Exceptions;
using Moq;
using System.Linq.Expressions;

namespace BlankoCRM.UnitTests.Appointments;

public sealed class AppointmentServiceCreationTests
{
    [Fact]
    public async Task CreateWithScheduleAsync_creates_a_scheduled_appointment_and_removes_conflicting_free_slots()
    {
        var context = new AppointmentServiceTestContext();
        var client = AppointmentTestData.Client();
        var service = AppointmentTestData.Service();
        var startTime = AppointmentTestData.At(9);
        var input = new ServiceInput(service.Id, Quantity: 2);
        Appointment? persistedAppointment = null;
        Schedule? persistedSchedule = null;
        context.ClientRepository
            .Setup(repository => repository.GetByIdAsync(client.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);
        context.ServiceRepository
            .Setup(repository => repository.FindAsync(
                It.IsAny<Expression<Func<Service, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<Service>)[service]);
        context.UnitOfWork
            .Setup(unitOfWork => unitOfWork.Add(It.IsAny<Appointment>()))
            .Callback<Appointment>(appointment => persistedAppointment = appointment);
        context.UnitOfWork
            .Setup(unitOfWork => unitOfWork.Add(It.IsAny<Schedule>()))
            .Callback<Schedule>(schedule => persistedSchedule = schedule);

        var result = await context.Service.CreateWithScheduleAsync(
            client.Id, [input], startTime, "notes");

        Assert.Equal(AppointmentStatus.Scheduled, result.Appointment.Status);
        Assert.Equal(client.Id, result.Appointment.ClientId);
        Assert.Equal(startTime, result.Appointment.StartTime);
        Assert.Equal(startTime.AddMinutes(60), result.Appointment.EndTime);
        Assert.Equal("notes", result.Appointment.Notes);
        var appointmentService = Assert.Single(result.Appointment.AppointmentServices);
        Assert.Equal(service.Id, appointmentService.ServiceId);
        Assert.Equal(service.Name, appointmentService.ServiceName);
        Assert.Equal(service.Price, appointmentService.Price);
        Assert.Equal(2, appointmentService.Quantity);
        Assert.Equal(60, appointmentService.DurationMinutes * appointmentService.Quantity);
        Assert.Same(result.Appointment, persistedAppointment);
        Assert.Same(result.Schedule, persistedSchedule);
        Assert.Equal(context.OrganizationId, result.Appointment.OrganizationId);
        context.ScheduleRepository.Verify(repository => repository.DeleteFreeSlotsByTimeAsync(
            startTime, startTime.AddMinutes(60), context.OrganizationId, null, null,
            It.IsAny<CancellationToken>()), Times.Once);
        context.Transaction.Verify(transaction => transaction.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateWithScheduleAsync_uses_price_and_duration_overrides_from_service_input()
    {
        var context = new AppointmentServiceTestContext();
        var client = AppointmentTestData.Client();
        var service = AppointmentTestData.Service(durationMinutes: 30, price: 50m);
        var startTime = AppointmentTestData.At(9);
        var input = new ServiceInput(service.Id, DurationMinutes: 45, Price: 75m, Quantity: 2);
        context.ClientRepository
            .Setup(repository => repository.GetByIdAsync(client.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);
        context.ServiceRepository
            .Setup(repository => repository.FindAsync(
                It.IsAny<Expression<Func<Service, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<Service>)[service]);

        var result = await context.Service.CreateWithScheduleAsync(client.Id, [input], startTime);

        var appointmentService = Assert.Single(result.Appointment.AppointmentServices);
        Assert.Equal(75m, appointmentService.Price);
        Assert.Equal(45, appointmentService.DurationMinutes);
        Assert.Equal(2, appointmentService.Quantity);
        Assert.Equal(startTime.AddMinutes(90), result.Appointment.EndTime);
        Assert.Equal(result.Appointment.EndTime, result.Schedule.EndTime);
    }

    [Fact]
    public async Task CreateWithScheduleAsync_throws_when_the_client_does_not_exist()
    {
        var context = new AppointmentServiceTestContext();
        var clientId = Guid.NewGuid();
        context.ClientRepository
            .Setup(repository => repository.GetByIdAsync(clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => context.Service.CreateWithScheduleAsync(
            clientId, [new ServiceInput(Guid.NewGuid())], AppointmentTestData.At(9)));

        context.UnitOfWork.Verify(unitOfWork => unitOfWork.Add(It.IsAny<Appointment>()), Times.Never);
    }

    [Fact]
    public async Task CreateWithScheduleAsync_throws_when_a_requested_service_does_not_exist()
    {
        var context = new AppointmentServiceTestContext();
        var client = AppointmentTestData.Client();
        context.ClientRepository
            .Setup(repository => repository.GetByIdAsync(client.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);
        context.ServiceRepository
            .Setup(repository => repository.FindAsync(
                It.IsAny<Expression<Func<Service, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<Service>)[]);

        await Assert.ThrowsAsync<NotFoundException>(() => context.Service.CreateWithScheduleAsync(
            client.Id, [new ServiceInput(Guid.NewGuid())], AppointmentTestData.At(9)));

        context.UnitOfWork.Verify(unitOfWork => unitOfWork.Add(It.IsAny<Appointment>()), Times.Never);
    }

    [Fact]
    public async Task CreateWithScheduleAsync_rejects_a_time_conflict()
    {
        var context = new AppointmentServiceTestContext();
        var client = AppointmentTestData.Client();
        var service = AppointmentTestData.Service();
        context.ClientRepository
            .Setup(repository => repository.GetByIdAsync(client.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);
        context.ServiceRepository
            .Setup(repository => repository.FindAsync(
                It.IsAny<Expression<Func<Service, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<Service>)[service]);
        context.AppointmentRepository
            .Setup(repository => repository.HasOverlapAsync(
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), context.OrganizationId,
                It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() => context.Service.CreateWithScheduleAsync(
            client.Id, [new ServiceInput(service.Id)], AppointmentTestData.At(9)));

        context.UnitOfWork.Verify(unitOfWork => unitOfWork.Add(It.IsAny<Appointment>()), Times.Never);
        context.ScheduleRepository.Verify(repository => repository.DeleteFreeSlotsByTimeAsync(
            It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<Guid>(), It.IsAny<Guid?>(),
            It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
