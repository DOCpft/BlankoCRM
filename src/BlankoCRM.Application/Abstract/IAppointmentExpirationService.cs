namespace BlankoCRM.Application.Abstract;

/// <summary>
/// Переводит истёкшие Pending-заявки в Cancelled.
/// </summary>
public interface IAppointmentExpirationService
{
    Task<int> CancelExpiredPendingAsync(CancellationToken cancellationToken = default);
}
