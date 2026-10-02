using BlankoCRM.Core.Entities;
using BlankoCRM.Core.Exceptions;
using Moq;
using System.Data;

namespace BlankoCRM.UnitTests.Scheduling;

public sealed class ScheduleServiceBulkSlotTests
{
    [Fact]
    public async Task AddSlotsAsync_returns_an_empty_list_without_accessing_dependencies_for_empty_input()
    {
        var context = new ScheduleServiceTestContext();

        var result = await context.Service.AddSlotsAsync([]);

        Assert.Empty(result);
        context.ScheduleRepository.Verify(
            repository => repository.AddRangeAsync(It.IsAny<IEnumerable<Schedule>>(), It.IsAny<CancellationToken>()),
            Times.Never);
        context.UnitOfWork.Verify(
            unitOfWork => unitOfWork.BeginTransactionAsync(It.IsAny<IsolationLevel>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task AddSlotsAsync_persists_non_overlapping_slots()
    {
        var context = new ScheduleServiceTestContext();
        var requests = new List<BlankoCRM.Application.DTOs.CreateFreeSlotRequest>
        {
            ScheduleTestData.FreeSlot(9, 10),
            ScheduleTestData.FreeSlot(10, 11)
        };
        List<Schedule>? addedSlots = null;
        context.ScheduleRepository
            .Setup(repository => repository.AddRangeAsync(
                It.IsAny<IEnumerable<Schedule>>(), It.IsAny<CancellationToken>()))
            .Callback<IEnumerable<Schedule>, CancellationToken>((slots, _) => addedSlots = slots.ToList())
            .Returns(Task.CompletedTask);

        var result = await context.Service.AddSlotsAsync(requests);

        Assert.Equal(2, result.Count);
        Assert.NotNull(addedSlots);
        Assert.Equal(2, addedSlots.Count);
        Assert.All(addedSlots, slot => Assert.Equal(context.OrganizationId, slot.OrganizationId));
        context.UnitOfWork.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        context.UnitOfWork.Verify(unitOfWork => unitOfWork.BeginTransactionAsync(
            IsolationLevel.Serializable, It.IsAny<CancellationToken>()), Times.Once);
        context.Transaction.Verify(transaction => transaction.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddSlotsAsync_rejects_overlaps_inside_the_batch()
    {
        var context = new ScheduleServiceTestContext();
        var requests = new List<BlankoCRM.Application.DTOs.CreateFreeSlotRequest>
        {
            ScheduleTestData.FreeSlot(9, 11),
            ScheduleTestData.FreeSlot(10, 12)
        };

        await Assert.ThrowsAsync<ConflictException>(() => context.Service.AddSlotsAsync(requests));

        context.ScheduleRepository.Verify(
            repository => repository.AddRangeAsync(It.IsAny<IEnumerable<Schedule>>(), It.IsAny<CancellationToken>()),
            Times.Never);
        context.UnitOfWork.Verify(
            unitOfWork => unitOfWork.BeginTransactionAsync(It.IsAny<IsolationLevel>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task AddSlotsAsync_rejects_an_overlap_with_an_existing_slot()
    {
        var context = new ScheduleServiceTestContext();
        context.ScheduleRepository
            .Setup(repository => repository.HasOverlapWithFreeSlotsAsync(
                It.IsAny<DateTime>(), It.IsAny<DateTime>(), context.OrganizationId,
                It.IsAny<Guid?>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() =>
            context.Service.AddSlotsAsync([ScheduleTestData.FreeSlot(9, 10)]));

        context.ScheduleRepository.Verify(
            repository => repository.AddRangeAsync(It.IsAny<IEnumerable<Schedule>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task AddSlotsAsync_rejects_a_slot_with_an_invalid_time_range()
    {
        var context = new ScheduleServiceTestContext();

        await Assert.ThrowsAsync<ValidationException>(() =>
            context.Service.AddSlotsAsync([ScheduleTestData.FreeSlot(11, 10)]));

        context.ScheduleRepository.Verify(
            repository => repository.AddRangeAsync(It.IsAny<IEnumerable<Schedule>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
