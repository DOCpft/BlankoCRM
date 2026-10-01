using BlankoCRM.Application.Abstract;
using BlankoCRM.Application.DTOs;
using BlankoCRM.Core.Exceptions;
using BlankoCRM.Core.Interfaces;

namespace BlankoCRM.Application.Services;

public sealed class PublicScheduleService : IPublicScheduleService
{
    private static readonly TimeSpan MaxPeriod = TimeSpan.FromDays(31);
    private const int MaxSlots = 500;
    private readonly IOrganizationContext _organizationContext;
    private readonly IScheduleRepository _scheduleRepository;

    public PublicScheduleService(
        IOrganizationContext organizationContext,
        IScheduleRepository scheduleRepository)
    {
        _organizationContext = organizationContext;
        _scheduleRepository = scheduleRepository;
    }

    public async Task<IReadOnlyList<PublicFreeSlotDto>> GetFreeSlotsAsync(
        DateTime from,
        DateTime to,
        int limit,
        CancellationToken cancellationToken = default)
    {
        if (from >= to)
        {
            throw new ValidationException("Начало периода должно быть раньше конца периода.");
        }

        if (to - from > MaxPeriod)
        {
            throw new ValidationException("За один запрос можно получить расписание не более чем на 31 день.");
        }

        if (limit is < 1 or > MaxSlots)
        {
            throw new ValidationException($"Лимит слотов должен быть от 1 до {MaxSlots}.");
        }

        var organizationId = _organizationContext.GetCurrentOrganizationId()
            ?? throw new UnauthorizedAccessException("Organization context is not initialized.");
        var now = DateTime.UtcNow;
        if (to <= now)
        {
            return [];
        }

        var slots = await _scheduleRepository.GetFreeSlotsByPeriodAsync(
            organizationId,
            from > now ? from : now,
            to,
            limit,
            cancellationToken);

        return slots
            .Select(slot => new PublicFreeSlotDto(slot.Id, slot.StartTime, slot.EndTime))
            .ToList();
    }
}
