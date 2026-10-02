using BlankoCRM.Application.Services;
using BlankoCRM.Core.Entities;
using BlankoCRM.Core.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using AppointmentAppService = BlankoCRM.Application.Services.AppointmentService;

namespace BlankoCRM.UnitTests.Appointments;

internal sealed class AppointmentServiceTestContext
{
    public Guid OrganizationId { get; } = Guid.NewGuid();
    public Mock<IAppointmentRepository> AppointmentRepository { get; } = new();
    public Mock<IRepository<Client>> ClientRepository { get; } = new();
    public Mock<IRepository<Service>> ServiceRepository { get; } = new();
    public Mock<IRepository<Salon>> SalonRepository { get; } = new();
    public Mock<IScheduleRepository> ScheduleRepository { get; } = new();
    public Mock<IUnitOfWork> UnitOfWork { get; } = new();
    public Mock<IOrganizationContext> OrganizationContext { get; } = new();
    public Mock<IUnitOfWorkTransaction> Transaction { get; } = new();
    public AppointmentAppService Service { get; }

    public AppointmentServiceTestContext()
    {
        OrganizationContext
            .Setup(context => context.GetCurrentOrganizationId())
            .Returns(OrganizationId);
        UnitOfWork
            .Setup(unitOfWork => unitOfWork.HasActiveTransaction)
            .Returns(false);
        UnitOfWork
            .Setup(unitOfWork => unitOfWork.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Transaction.Object);
        Transaction
            .Setup(transaction => transaction.DisposeAsync())
            .Returns(ValueTask.CompletedTask);

        Service = new AppointmentAppService(
            AppointmentRepository.Object,
            ClientRepository.Object,
            ServiceRepository.Object,
            SalonRepository.Object,
            ScheduleRepository.Object,
            UnitOfWork.Object,
            OrganizationContext.Object,
            NullLogger<AppointmentAppService>.Instance);
    }
}
