using BlankoCRM.Application.Abstract;
using BlankoCRM.Application.DTOs;
using BlankoCRM.Application.Services;
using BlankoCRM.Core.Entities;
using BlankoCRM.Core.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace BlankoCRM.UnitTests.PublicAppointments;

internal sealed class PublicAppointmentTestContext
{
    public const long VkUserId = 24680;
    public Guid OrganizationId { get; } = Guid.NewGuid();
    public Dictionary<string, string> VkParams { get; } = new() { ["sign"] = "valid" };
    public Mock<IUnitOfWork> UnitOfWork { get; } = new();
    public Mock<IOrganizationContext> OrganizationContext { get; } = new();
    public Mock<IVkMiniAppAuthenticator> Authenticator { get; } = new();
    public Mock<IPublicVkRateLimiter> RateLimiter { get; } = new();
    public Mock<IPublicClientResolver> ClientResolver { get; } = new();
    public Mock<IAppointmentRepository> AppointmentRepository { get; } = new();
    public Mock<IClientRepository> ClientRepository { get; } = new();
    public Mock<IScheduleRepository> ScheduleRepository { get; } = new();
    public Mock<IRepository<Service>> ServiceRepository { get; } = new();
    public Mock<ITelegramNotificationService> TelegramNotificationService { get; } = new();
    public Mock<IUnitOfWorkTransaction> Transaction { get; } = new();
    public PublicAppointmentService Service { get; }

    public PublicAppointmentTestContext()
    {
        OrganizationContext
            .Setup(context => context.GetCurrentOrganizationId())
            .Returns(OrganizationId);
        Authenticator
            .Setup(authenticator => authenticator.Authenticate(It.IsAny<IReadOnlyDictionary<string, string>>()))
            .Returns(VkUserId);
        RateLimiter
            .Setup(rateLimiter => rateLimiter.TryAcquire(It.IsAny<long>(), It.IsAny<PublicAppointmentOperation>()))
            .Returns(true);
        UnitOfWork
            .Setup(unitOfWork => unitOfWork.HasActiveTransaction)
            .Returns(false);
        UnitOfWork
            .Setup(unitOfWork => unitOfWork.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Transaction.Object);
        UnitOfWork
            .Setup(unitOfWork => unitOfWork.IsUniqueViolation(It.IsAny<Exception>()))
            .Returns(false);
        Transaction
            .Setup(transaction => transaction.DisposeAsync())
            .Returns(ValueTask.CompletedTask);
        TelegramNotificationService
            .Setup(service => service.NotifyMasterNewPendingAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        Service = new PublicAppointmentService(
            UnitOfWork.Object,
            OrganizationContext.Object,
            Authenticator.Object,
            RateLimiter.Object,
            ClientResolver.Object,
            AppointmentRepository.Object,
            ClientRepository.Object,
            ScheduleRepository.Object,
            ServiceRepository.Object,
            TelegramNotificationService.Object,
            NullLogger<PublicAppointmentService>.Instance);
    }

    public CreatePublicAppointmentRequest CreateValidRequest(
        Guid? freeSlotId = null,
        DateTimeOffset? startTime = null,
        List<PublicServiceInput>? services = null,
        string name = "Client") => new(
            freeSlotId ?? Guid.NewGuid(),
            new Dictionary<string, string>(VkParams),
            name,
            null,
            services ?? [new PublicServiceInput(Guid.NewGuid())],
            startTime ?? DateTimeOffset.UtcNow.AddDays(2),
            null);

    public void ConfigureValidCreate(
        CreatePublicAppointmentRequest request,
        Client client,
        Schedule freeSlot,
        params Service[] services)
    {
        ServiceRepository
            .Setup(repository => repository.FindAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Service, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<Service>)services);
        ScheduleRepository
            .Setup(repository => repository.GetFreeSlotByIdAsync(
                OrganizationId, request.FreeSlotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(freeSlot);
        ClientResolver
            .Setup(resolver => resolver.ResolveAsync(
                OrganizationId,
                VkUserId,
                request.Name,
                request.Phone,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);
        AppointmentRepository
            .Setup(repository => repository.CountActivePendingAsync(
                OrganizationId, client.Id, It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        AppointmentRepository
            .Setup(repository => repository.HasOverlappingActivePendingAsync(
                OrganizationId, client.Id, It.IsAny<DateTime>(), It.IsAny<DateTime>(),
                It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
    }
}
