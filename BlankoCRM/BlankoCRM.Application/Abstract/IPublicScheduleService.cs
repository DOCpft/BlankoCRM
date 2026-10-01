using BlankoCRM.Application.DTOs;

namespace BlankoCRM.Application.Abstract;

public interface IPublicScheduleService
{
    Task<IReadOnlyList<PublicFreeSlotDto>> GetFreeSlotsAsync(
        DateTime from,
        DateTime to,
        int limit,
        CancellationToken cancellationToken = default);
}
