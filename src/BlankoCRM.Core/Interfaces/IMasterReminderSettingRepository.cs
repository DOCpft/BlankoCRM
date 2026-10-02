using BlankoCRM.Core.Entities;

namespace BlankoCRM.Core.Interfaces;

/// <summary>
/// Репозиторий настроек напоминаний мастера.
/// </summary>
public interface IMasterReminderSettingRepository : IRepository<MasterReminderSetting>
{
    /// <summary>
    /// Возвращает все настройки напоминаний указанного пользователя.
    /// </summary>
    Task<IReadOnlyList<MasterReminderSetting>> GetByAppUserIdAsync(
        Guid appUserId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Проверяет, включено ли напоминание с указанным offset у пользователя.
    /// </summary>
    Task<bool> IsEnabledAsync(
        Guid appUserId,
        int offsetHours,
        CancellationToken cancellationToken = default);
}
