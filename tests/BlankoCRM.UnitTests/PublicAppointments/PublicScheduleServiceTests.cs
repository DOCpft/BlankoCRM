using BlankoCRM.Application.Services;
using BlankoCRM.Core.Entities;
using BlankoCRM.Core.Exceptions;
using BlankoCRM.Core.Interfaces;
using Moq;

namespace BlankoCRM.UnitTests.PublicAppointments;

public sealed class PublicScheduleServiceTests
{
    [Fact]
    public async Task GetFreeSlotsAsync_returns_free_slots_for_the_current_organization()
    {
        var organizationContext = new Mock<IOrganizationContext>();
        var scheduleRepository = new Mock<IScheduleRepository>();
        var organizationId = Guid.NewGuid();
        var from = DateTime.UtcNow.AddHours(1);
        var to = from.AddDays(2);
        var slot = new Schedule
        {
            Id = Guid.NewGuid(),
            StartTime = from.AddHours(1),
            EndTime = from.AddHours(2)
        };
        organizationContext.Setup(context => context.GetCurrentOrganizationId()).Returns(organizationId);
        scheduleRepository
            .Setup(repository => repository.GetFreeSlotsByPeriodAsync(
                organizationId, from, to, 25, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<Schedule>)[slot]);
        var service = new PublicScheduleService(organizationContext.Object, scheduleRepository.Object);

        var result = await service.GetFreeSlotsAsync(from, to, 25);

        var resultSlot = Assert.Single(result);
        Assert.Equal(slot.Id, resultSlot.Id);
        Assert.Equal(slot.StartTime, resultSlot.StartTime);
        Assert.Equal(slot.EndTime, resultSlot.EndTime);
    }

    [Fact]
    public async Task GetFreeSlotsAsync_clamps_a_past_start_to_the_current_time()
    {
        var organizationContext = new Mock<IOrganizationContext>();
        var scheduleRepository = new Mock<IScheduleRepository>();
        var organizationId = Guid.NewGuid();
        var from = DateTime.UtcNow.AddDays(-1);
        var to = DateTime.UtcNow.AddDays(1);
        var beforeCall = DateTime.UtcNow;
        organizationContext.Setup(context => context.GetCurrentOrganizationId()).Returns(organizationId);
        scheduleRepository
            .Setup(repository => repository.GetFreeSlotsByPeriodAsync(
                organizationId,
                It.Is<DateTime>(start => start >= beforeCall && start <= DateTime.UtcNow),
                to,
                25,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyList<Schedule>)[]);
        var service = new PublicScheduleService(organizationContext.Object, scheduleRepository.Object);

        await service.GetFreeSlotsAsync(from, to, 25);

        scheduleRepository.Verify(repository => repository.GetFreeSlotsByPeriodAsync(
            organizationId,
            It.Is<DateTime>(start => start >= beforeCall && start <= DateTime.UtcNow),
            to,
            25,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetFreeSlotsAsync_returns_empty_without_querying_past_periods()
    {
        var organizationContext = new Mock<IOrganizationContext>();
        var scheduleRepository = new Mock<IScheduleRepository>();
        organizationContext.Setup(context => context.GetCurrentOrganizationId()).Returns(Guid.NewGuid());
        var service = new PublicScheduleService(organizationContext.Object, scheduleRepository.Object);

        var result = await service.GetFreeSlotsAsync(
            DateTime.UtcNow.AddDays(-3), DateTime.UtcNow.AddDays(-2), 25);

        Assert.Empty(result);
        scheduleRepository.Verify(repository => repository.GetFreeSlotsByPeriodAsync(
            It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(10, 10)]
    [InlineData(11, 10)]
    public async Task GetFreeSlotsAsync_rejects_an_invalid_period(int fromHour, int toHour)
    {
        var organizationContext = new Mock<IOrganizationContext>();
        var scheduleRepository = new Mock<IScheduleRepository>();
        var from = new DateTime(2032, 2, 3, fromHour, 0, 0, DateTimeKind.Utc);
        var to = new DateTime(2032, 2, 3, toHour, 0, 0, DateTimeKind.Utc);
        var service = new PublicScheduleService(organizationContext.Object, scheduleRepository.Object);

        await Assert.ThrowsAsync<ValidationException>(() => service.GetFreeSlotsAsync(from, to, 25));
    }

    [Fact]
    public async Task GetFreeSlotsAsync_rejects_a_period_longer_than_31_days()
    {
        var service = new PublicScheduleService(
            new Mock<IOrganizationContext>().Object,
            new Mock<IScheduleRepository>().Object);

        await Assert.ThrowsAsync<ValidationException>(() => service.GetFreeSlotsAsync(
            DateTime.UtcNow, DateTime.UtcNow.AddDays(32), 25));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(501)]
    public async Task GetFreeSlotsAsync_rejects_a_limit_outside_the_supported_range(int limit)
    {
        var service = new PublicScheduleService(
            new Mock<IOrganizationContext>().Object,
            new Mock<IScheduleRepository>().Object);

        await Assert.ThrowsAsync<ValidationException>(() => service.GetFreeSlotsAsync(
            DateTime.UtcNow, DateTime.UtcNow.AddDays(1), limit));
    }
}
