using BlankoCRM.Application.DTOs;

namespace BlankoCRM.Application.Abstract;

/// <summary>
/// Сервис управления настройками напоминаний мастера.
/// </summary>
public interface IReminderSettingService
{
    /// <summary>
    /// Возвращает все настройки напоминаний текущего мастера.
    /// </summary>
    Task<List<ReminderSettingDto>> GetAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Обновляет флаги включения переданных offset'ов.
    /// </summary>
    Task UpdateAsync(UpdateReminderSettingsRequest request, CancellationToken ct = default);
}
