using BlankoCRM.Application.DTOs;
using BlankoCRM.Core.Entities;
using BlankoCRM.Core.Exceptions;
using Moq;
using System.Linq.Expressions;

namespace BlankoCRM.UnitTests.Appointments;

public sealed class AppointmentServiceUpdateDeleteTests
{
    [Fact]
    public async Task UpdateAsync_updates_the_appointment_and_its_schedule()
    {
        var context = new AppointmentServiceTestContext();
        var appointment = AppointmentTestData.Appointment();
        var service = AppointmentTestData.Service(45, 70m);
        appointment.AppointmentServices.Add(new AppointmentService
        {
            ServiceId = Guid.NewGuid(),
            DurationMinutes = 30,
            Quantity = 1
        });
        var schedule = new Schedule
        {
            Id = Guid.NewGuid(),
            AppointmentId = appointment.Id,
            StartTime = appointment.StartTime,
            EndTime = appointment.EndTime
        };
        context.AppointmentRepository
            .Setup(repository => repository.GetByIdWithDetailsAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        context.ScheduleRepository
            .Setup(repository => repository.GetSlotByAppointmentIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);
        context.ServiceRepository
            .Setup(repository => repository.FindAsync(
                It.IsAny<Expression<Func<Service, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<Service>)[service]);

        var newStartTime = AppointmentTestData.At(11);
        await context.Service.UpdateAsync(appointment.Id, new UpdateAppointmentRequest(
            null, [new ServiceInput(service.Id, Quantity: 2)], newStartTime, null, "updated notes"));

        Assert.Equal(newStartTime, appointment.StartTime);
        Assert.Equal(newStartTime.AddMinutes(90), appointment.EndTime);
        Assert.Equal(appointment.StartTime, schedule.StartTime);
        Assert.Equal(appointment.EndTime, schedule.EndTime);
        Assert.Equal("updated notes", appointment.Notes);
        var updatedService = Assert.Single(appointment.AppointmentServices);
        Assert.Equal(service.Id, updatedService.ServiceId);
        Assert.Equal(2, updatedService.Quantity);
        Assert.Equal(70m, updatedService.Price);
        context.UnitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_rejects_a_pending_appointment()
    {
        var context = new AppointmentServiceTestContext();
        var appointment = AppointmentTestData.Appointment(AppointmentStatus.Pending);
        context.AppointmentRepository
            .Setup(repository => repository.GetByIdWithDetailsAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        await Assert.ThrowsAsync<ConflictException>(() => context.Service.UpdateAsync(
            appointment.Id, new UpdateAppointmentRequest(null, null, null, null, "notes")));

        context.UnitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_rejects_a_time_conflict_without_mutating_the_appointment()
    {
        var context = new AppointmentServiceTestContext();
        var appointment = AppointmentTestData.Appointment();
        appointment.AppointmentServices.Add(new AppointmentService { DurationMinutes = 30, Quantity = 1 });
        var originalStartTime = appointment.StartTime;
        context.AppointmentRepository
            .Setup(repository => repository.GetByIdWithDetailsAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        context.AppointmentRepository
            .Setup(repository => repository.HasOverlapAsync(
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), appointment.OrganizationId,
                appointment.SalonId, appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() => context.Service.UpdateAsync(
            appointment.Id, new UpdateAppointmentRequest(null, null, AppointmentTestData.At(11), null, null)));

        Assert.Equal(originalStartTime, appointment.StartTime);
        context.UnitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_archives_the_appointment_and_releases_its_schedule()
    {
        var context = new AppointmentServiceTestContext();
        var appointment = AppointmentTestData.Appointment();
        var schedule = new Schedule { Id = Guid.NewGuid(), AppointmentId = appointment.Id, Appointment = appointment };
        context.AppointmentRepository
            .Setup(repository => repository.GetByIdWithDetailsAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        context.ScheduleRepository
            .Setup(repository => repository.GetSlotByAppointmentIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);

        await context.Service.DeleteAsync(appointment.Id);

        Assert.Equal(AppointmentStatus.Archived, appointment.Status);
        Assert.Null(schedule.AppointmentId);
        Assert.Null(schedule.Appointment);
        context.UnitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_rejects_a_pending_appointment()
    {
        var context = new AppointmentServiceTestContext();
        var appointment = AppointmentTestData.Appointment(AppointmentStatus.Pending);
        context.AppointmentRepository
            .Setup(repository => repository.GetByIdWithDetailsAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        await Assert.ThrowsAsync<ConflictException>(() => context.Service.DeleteAsync(appointment.Id));

        context.UnitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_throws_when_the_appointment_does_not_exist()
    {
        var context = new AppointmentServiceTestContext();
        var appointmentId = Guid.NewGuid();
        context.AppointmentRepository
            .Setup(repository => repository.GetByIdWithDetailsAsync(appointmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => context.Service.DeleteAsync(appointmentId));
    }
}
