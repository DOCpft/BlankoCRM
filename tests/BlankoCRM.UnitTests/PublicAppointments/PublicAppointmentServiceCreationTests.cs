using BlankoCRM.Application.DTOs;
using BlankoCRM.Application.Abstract;
using BlankoCRM.Core.Entities;
using BlankoCRM.Core.Exceptions;
using Moq;

namespace BlankoCRM.UnitTests.PublicAppointments;

public sealed class PublicAppointmentServiceCreationTests
{
    [Fact]
    public async Task CreatePendingAsync_creates_an_expiring_pending_request_and_notifies_the_master()
    {
        var scenario = CreateValidScenario(quantity: 2);
        Appointment? addedAppointment = null;
        scenario.Context.UnitOfWork
            .Setup(unitOfWork => unitOfWork.Add(It.IsAny<Appointment>()))
            .Callback<Appointment>(appointment => addedAppointment = appointment);

        var result = await scenario.Context.Service.CreatePendingAsync(scenario.Request);

        Assert.Equal(AppointmentStatus.Pending, result.Status);
        Assert.Equal(scenario.Client.Id, result.ClientId);
        Assert.Equal(scenario.Request.StartTime.UtcDateTime, result.StartTime);
        Assert.Equal(scenario.Request.StartTime.UtcDateTime.AddMinutes(60), result.EndTime);
        Assert.Equal(scenario.Request.Notes, result.Notes);
        Assert.NotNull(result.ExpiresAt);
        Assert.InRange(result.ExpiresAt.Value - DateTime.UtcNow, TimeSpan.FromHours(23), TimeSpan.FromHours(25));
        var appointmentService = Assert.Single(result.AppointmentServices);
        Assert.Equal(scenario.Service.Id, appointmentService.ServiceId);
        Assert.Equal(2, appointmentService.Quantity);
        Assert.Same(result, addedAppointment);
        scenario.Context.TelegramNotificationService.Verify(notification => notification.NotifyMasterNewPendingAsync(
            result, It.IsAny<CancellationToken>()), Times.Once);
        scenario.Context.ScheduleRepository.Verify(repository => repository.LockFreeSlotForUpdateAsync(
            scenario.Context.OrganizationId, scenario.Request.FreeSlotId, It.IsAny<CancellationToken>()), Times.Once);
        scenario.Context.UnitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        scenario.Context.Transaction.Verify(transaction => transaction.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreatePendingAsync_propagates_an_invalid_vk_signature()
    {
        var context = new PublicAppointmentTestContext();
        var request = context.CreateValidRequest();
        context.Authenticator
            .Setup(authenticator => authenticator.Authenticate(It.IsAny<IReadOnlyDictionary<string, string>>()))
            .Throws(new UnauthorizedException("Invalid VK signature"));

        await Assert.ThrowsAsync<UnauthorizedException>(() => context.Service.CreatePendingAsync(request));

        context.RateLimiter.Verify(
            rateLimiter => rateLimiter.TryAcquire(It.IsAny<long>(), It.IsAny<PublicAppointmentOperation>()),
            Times.Never);
        context.ServiceRepository.Verify(repository => repository.FindAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Service, bool>>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreatePendingAsync_rejects_a_past_start_time()
    {
        var context = new PublicAppointmentTestContext();
        var request = context.CreateValidRequest(startTime: DateTimeOffset.UtcNow.AddHours(-1));

        await Assert.ThrowsAsync<ValidationException>(() => context.Service.CreatePendingAsync(request));

        context.ServiceRepository.Verify(repository => repository.FindAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Service, bool>>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreatePendingAsync_rejects_a_service_not_found_in_the_current_organization()
    {
        var context = new PublicAppointmentTestContext();
        var request = context.CreateValidRequest();
        context.ServiceRepository
            .Setup(repository => repository.FindAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Service, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<Service>)[]);

        await Assert.ThrowsAsync<NotFoundException>(() => context.Service.CreatePendingAsync(request));

        context.ScheduleRepository.Verify(repository => repository.LockFreeSlotForUpdateAsync(
            It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreatePendingAsync_rejects_a_slot_that_is_no_longer_available()
    {
        var scenario = CreateValidScenario();
        scenario.Context.ScheduleRepository
            .Setup(repository => repository.GetFreeSlotByIdAsync(
                scenario.Context.OrganizationId,
                scenario.Request.FreeSlotId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Schedule?)null);

        await Assert.ThrowsAsync<ConflictException>(
            () => scenario.Context.Service.CreatePendingAsync(scenario.Request));

        scenario.Context.UnitOfWork.Verify(unitOfWork => unitOfWork.Add(It.IsAny<Appointment>()), Times.Never);
    }

    [Fact]
    public async Task CreatePendingAsync_rejects_a_request_that_does_not_fit_inside_the_selected_slot()
    {
        var scenario = CreateValidScenario();
        scenario.Slot.StartTime = scenario.Request.StartTime.UtcDateTime.AddMinutes(1);

        await Assert.ThrowsAsync<ValidationException>(
            () => scenario.Context.Service.CreatePendingAsync(scenario.Request));

        scenario.Context.UnitOfWork.Verify(unitOfWork => unitOfWork.Add(It.IsAny<Appointment>()), Times.Never);
    }

    [Fact]
    public async Task CreatePendingAsync_rejects_a_client_with_the_maximum_active_pending_requests()
    {
        var scenario = CreateValidScenario();
        scenario.Context.AppointmentRepository
            .Setup(repository => repository.CountActivePendingAsync(
                scenario.Context.OrganizationId, scenario.Client.Id,
                It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        await Assert.ThrowsAsync<TooManyRequestsException>(
            () => scenario.Context.Service.CreatePendingAsync(scenario.Request));

        scenario.Context.AppointmentRepository.Verify(repository => repository.HasOverlappingActivePendingAsync(
            It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(),
            It.IsAny<DateTime>(), It.IsAny<CancellationToken>()), Times.Never);
        scenario.Context.UnitOfWork.Verify(unitOfWork => unitOfWork.Add(It.IsAny<Appointment>()), Times.Never);
    }

    [Fact]
    public async Task CreatePendingAsync_rejects_a_duplicate_pending_request_for_the_same_time()
    {
        var scenario = CreateValidScenario();
        scenario.Context.AppointmentRepository
            .Setup(repository => repository.HasOverlappingActivePendingAsync(
                scenario.Context.OrganizationId, scenario.Client.Id,
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(
            () => scenario.Context.Service.CreatePendingAsync(scenario.Request));

        scenario.Context.UnitOfWork.Verify(unitOfWork => unitOfWork.Add(It.IsAny<Appointment>()), Times.Never);
    }

    [Fact]
    public async Task CreatePendingAsync_rejects_requests_when_the_vk_rate_limit_is_exceeded()
    {
        var context = new PublicAppointmentTestContext();
        var request = context.CreateValidRequest();
        context.RateLimiter
            .Setup(rateLimiter => rateLimiter.TryAcquire(
                PublicAppointmentTestContext.VkUserId, PublicAppointmentOperation.Create))
            .Returns(false);

        await Assert.ThrowsAsync<TooManyRequestsException>(() => context.Service.CreatePendingAsync(request));

        context.ServiceRepository.Verify(repository => repository.FindAsync(
            It.IsAny<System.Linq.Expressions.Expression<Func<Service, bool>>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    private static (PublicAppointmentTestContext Context, CreatePublicAppointmentRequest Request,
        Service Service, Client Client, Schedule Slot) CreateValidScenario(int quantity = 1)
    {
        var context = new PublicAppointmentTestContext();
        var service = PublicAppointmentTestData.CreateService();
        var client = PublicAppointmentTestData.CreateClient();
        var request = context.CreateValidRequest(services: [new PublicServiceInput(service.Id, quantity)]);
        var slot = PublicAppointmentTestData.CreateFreeSlot(request);
        context.ConfigureValidCreate(request, client, slot, service);
        return (context, request, service, client, slot);
    }
}
