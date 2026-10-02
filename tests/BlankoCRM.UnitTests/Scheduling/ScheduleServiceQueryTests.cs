using BlankoCRM.Core.Entities;
using BlankoCRM.Core.Exceptions;
using Moq;

namespace BlankoCRM.UnitTests.Scheduling;

public sealed class ScheduleServiceQueryTests
{
    [Fact]
    public async Task GetSlotByIdAsync_returns_the_matching_slot()
    {
        var context = new ScheduleServiceTestContext();
        var slot = ScheduleTestData.Slot(9, 10);
        context.ScheduleRepository
            .Setup(repository => repository.GetByIdWithAppointmentAsync(slot.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(slot);

        var result = await context.Service.GetSlotByIdAsync(slot.Id);

        Assert.Same(slot, result);
    }

    [Fact]
    public async Task GetSlotByIdAsync_throws_when_the_slot_does_not_exist()
    {
        var context = new ScheduleServiceTestContext();
        var slotId = Guid.NewGuid();
        context.ScheduleRepository
            .Setup(repository => repository.GetByIdWithAppointmentAsync(slotId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Schedule?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => context.Service.GetSlotByIdAsync(slotId));
    }

    [Fact]
    public async Task GetScheduleByPeriodAsync_returns_slots_for_the_requested_period()
    {
        var context = new ScheduleServiceTestContext();
        var from = ScheduleTestData.At(8);
        var to = ScheduleTestData.At(18);
        IReadOnlyList<Schedule> slots = [ScheduleTestData.Slot(9, 10), ScheduleTestData.Slot(11, 12)];
        context.ScheduleRepository
            .Setup(repository => repository.GetByPeriodAsync(from, to, It.IsAny<CancellationToken>()))
            .ReturnsAsync(slots);

        var result = await context.Service.GetScheduleByPeriodAsync(from, to);

        Assert.Equal(slots, result);
        context.ScheduleRepository.Verify(
            repository => repository.GetByPeriodAsync(from, to, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
