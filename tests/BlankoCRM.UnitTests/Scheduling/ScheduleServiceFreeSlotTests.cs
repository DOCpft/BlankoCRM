using BlankoCRM.Core.Entities;
using BlankoCRM.Core.Exceptions;
using Moq;

namespace BlankoCRM.UnitTests.Scheduling;

public sealed class ScheduleServiceFreeSlotTests
{
    [Fact]
    public async Task AddFreeSlotAsync_persists_a_valid_free_slot_for_the_current_organization()
    {
        var context = new ScheduleServiceTestContext();
        var request = ScheduleTestData.FreeSlot(9, 10);
        context.ScheduleRepository
            .Setup(repository => repository.AddAsync(It.IsAny<Schedule>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Schedule slot, CancellationToken _) => slot);

        var result = await context.Service.AddFreeSlotAsync(request);

        Assert.Equal(context.OrganizationId, result.OrganizationId);
        Assert.Null(result.AppointmentId);
        Assert.Equal(request.StartTime, result.StartTime);
        Assert.Equal(request.EndTime, result.EndTime);
        context.ScheduleRepository.Verify(
            repository => repository.AddAsync(It.IsAny<Schedule>(), It.IsAny<CancellationToken>()),
            Times.Once);
        context.UnitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        context.Transaction.Verify(transaction => transaction.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(10, 10)]
    [InlineData(11, 10)]
    public async Task AddFreeSlotAsync_rejects_an_invalid_time_range(int startHour, int endHour)
    {
        var context = new ScheduleServiceTestContext();

        await Assert.ThrowsAsync<ValidationException>(
            () => context.Service.AddFreeSlotAsync(ScheduleTestData.FreeSlot(startHour, endHour)));

        context.ScheduleRepository.Verify(
            repository => repository.AddAsync(It.IsAny<Schedule>(), It.IsAny<CancellationToken>()),
            Times.Never);
        context.UnitOfWork.Verify(
            unitOfWork => unitOfWork.BeginTransactionAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task AddFreeSlotAsync_rejects_an_overlap_with_an_existing_free_slot()
    {
        var context = new ScheduleServiceTestContext();
        context.ScheduleRepository
            .Setup(repository => repository.HasOverlapWithFreeSlotsAsync(
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), context.OrganizationId,
                It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(
            () => context.Service.AddFreeSlotAsync(ScheduleTestData.FreeSlot(9, 10)));

        context.ScheduleRepository.Verify(
            repository => repository.AddAsync(It.IsAny<Schedule>(), It.IsAny<CancellationToken>()),
            Times.Never);
        context.UnitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AddFreeSlotAsync_rejects_an_overlap_with_an_existing_appointment()
    {
        var context = new ScheduleServiceTestContext();
        context.AppointmentRepository
            .Setup(repository => repository.HasOverlapAsync(
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), context.OrganizationId,
                It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(
            () => context.Service.AddFreeSlotAsync(ScheduleTestData.FreeSlot(9, 10)));

        context.ScheduleRepository.Verify(
            repository => repository.AddAsync(It.IsAny<Schedule>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
