using BlankoCRM.Application.Abstract;
using BlankoCRM.Application.Services;
using BlankoCRM.Core.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Data;

namespace BlankoCRM.UnitTests.Scheduling;

internal sealed class ScheduleServiceTestContext
{
    public Guid OrganizationId { get; } = Guid.NewGuid();
    public Mock<IScheduleRepository> ScheduleRepository { get; } = new();
    public Mock<IAppointmentRepository> AppointmentRepository { get; } = new();
    public Mock<IAppointmentService> AppointmentService { get; } = new();
    public Mock<IOrganizationContext> OrganizationContext { get; } = new();
    public Mock<IUnitOfWork> UnitOfWork { get; } = new();
    public Mock<IUnitOfWorkTransaction> Transaction { get; } = new();
    public ScheduleService Service { get; }

    public ScheduleServiceTestContext()
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
        UnitOfWork
            .Setup(unitOfWork => unitOfWork.BeginTransactionAsync(
                It.IsAny<IsolationLevel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Transaction.Object);
        Transaction
            .Setup(transaction => transaction.DisposeAsync())
            .Returns(ValueTask.CompletedTask);

        Service = new ScheduleService(
            ScheduleRepository.Object,
            AppointmentRepository.Object,
            AppointmentService.Object,
            OrganizationContext.Object,
            UnitOfWork.Object,
            NullLogger<ScheduleService>.Instance);
    }
}
