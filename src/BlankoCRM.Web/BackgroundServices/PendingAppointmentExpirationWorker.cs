using BlankoCRM.Application.Abstract;

namespace BlankoCRM.Web.BackgroundServices;

/// <summary>
/// Периодически запускает обработку просроченных клиентских заявок.
/// </summary>
public sealed class PendingAppointmentExpirationWorker : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PendingAppointmentExpirationWorker> _logger;

    public PendingAppointmentExpirationWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<PendingAppointmentExpirationWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var expirationService = scope.ServiceProvider
                    .GetRequiredService<IAppointmentExpirationService>();
                var cancelledCount = await expirationService.CancelExpiredPendingAsync(stoppingToken);

                if (cancelledCount > 0)
                {
                    _logger.LogInformation(
                        "Cancelled {AppointmentCount} expired pending appointments",
                        cancelledCount);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to cancel expired pending appointments");
            }

            try
            {
                await Task.Delay(Interval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }
}
