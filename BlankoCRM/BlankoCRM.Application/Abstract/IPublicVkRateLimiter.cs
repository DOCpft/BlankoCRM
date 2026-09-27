namespace BlankoCRM.Application.Abstract;

public enum PublicAppointmentOperation
{
    Create,
    Withdraw
}

/// <summary>
/// Ограничивает частоту публичных операций по подтверждённому VK user ID.
/// </summary>
public interface IPublicVkRateLimiter
{
    bool TryAcquire(long vkUserId, PublicAppointmentOperation operation);
}
