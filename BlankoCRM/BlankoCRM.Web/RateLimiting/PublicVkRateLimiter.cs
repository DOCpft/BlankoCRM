using BlankoCRM.Application.Abstract;
using System.Threading.RateLimiting;

namespace BlankoCRM.Web.RateLimiting;

/// <summary>
/// Fixed-window limiters partitioned by the VK ID already verified by the application service.
/// </summary>
public sealed class PublicVkRateLimiter : IPublicVkRateLimiter, IDisposable
{
    private readonly PartitionedRateLimiter<long> _createLimiter = CreateLimiter(5);
    private readonly PartitionedRateLimiter<long> _withdrawLimiter = CreateLimiter(10);

    public bool TryAcquire(long vkUserId, PublicAppointmentOperation operation)
    {
        using var lease = operation switch
        {
            PublicAppointmentOperation.Create => _createLimiter.AttemptAcquire(vkUserId),
            PublicAppointmentOperation.Withdraw => _withdrawLimiter.AttemptAcquire(vkUserId),
            _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
        };

        return lease.IsAcquired;
    }

    public void Dispose()
    {
        _createLimiter.Dispose();
        _withdrawLimiter.Dispose();
    }

    private static PartitionedRateLimiter<long> CreateLimiter(int permitsPerMinute)
    {
        return PartitionedRateLimiter.Create<long, long>(vkUserId =>
            RateLimitPartition.GetFixedWindowLimiter(
                vkUserId,
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = permitsPerMinute,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0,
                    AutoReplenishment = true
                }));
    }
}
