namespace BlankoCRM.Application.DTOs;

/// <summary>
/// Настройка напоминания мастера.
/// </summary>
/// <param name="OffsetHours">Количество часов до начала записи.</param>
/// <param name="Enabled">Включено ли напоминание.</param>
public record ReminderSettingDto(int OffsetHours, bool Enabled);

/// <summary>
/// Запрос на обновление настроек напоминаний.
/// </summary>
/// <param name="Settings">Список настроек для обновления.</param>
public record UpdateReminderSettingsRequest(List<ReminderSettingDto> Settings);
