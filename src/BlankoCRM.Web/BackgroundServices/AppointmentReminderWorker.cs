using BlankoCRM.Application.Abstract;

namespace BlankoCRM.Web.BackgroundServices;

/// <summary>
/// Периодически запускает рассылку напоминаний о предстоящих записях.
/// </summary>
public sealed class AppointmentReminderWorker : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AppointmentReminderWorker> _logger;

    public AppointmentReminderWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<AppointmentReminderWorker> logger)
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
                var dispatchService = scope.ServiceProvider
                    .GetRequiredService<IReminderDispatchService>();
                var sentCount = await dispatchService.DispatchDueRemindersAsync(stoppingToken);

                if (sentCount > 0)
                {
                    _logger.LogInformation("Отправлено напоминаний о записях: {ReminderCount}", sentCount);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Не удалось выполнить рассылку напоминаний о записях.");
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
