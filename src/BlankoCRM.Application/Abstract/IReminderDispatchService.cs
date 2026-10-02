namespace BlankoCRM.Application.Abstract;

/// <summary>
/// Сервис рассылки напоминаний о предстоящих записях.
/// </summary>
public interface IReminderDispatchService
{
    /// <summary>
    /// Находит записи, для которых подошло время напоминания, и отправляет напоминания.
    /// Возвращает количество отправленных напоминаний.
    /// </summary>
    Task<int> DispatchDueRemindersAsync(CancellationToken ct = default);
}
