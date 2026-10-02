using BlankoCRM.Core.Entities;

namespace BlankoCRM.Application.Abstract;

/// <summary>
/// Сервис формирования и отправки Telegram-уведомлений мастеру.
/// </summary>
public interface ITelegramNotificationService
{
    /// <summary>
    /// Уведомляет мастера о новой Pending-заявке клиента.
    /// </summary>
    Task NotifyMasterNewPendingAsync(
        Appointment appointment,
        CancellationToken ct = default);

    /// <summary>
    /// Отправляет напоминание о предстоящей записи за указанное число часов.
    /// </summary>
    Task SendReminderAsync(
        Appointment appointment,
        int offsetHours,
        CancellationToken ct = default);
}
