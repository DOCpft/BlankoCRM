using BlankoCRM.Core.Interfaces;

namespace BlankoCRM.Core.Entities;

/// <summary>
/// Настройка напоминания мастера: за сколько часов до записи отправлять напоминание в Telegram.
/// </summary>
public sealed class MasterReminderSetting : IOrganizationEntity
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid AppUserId { get; set; }

    /// <summary>
    /// Количество часов до начала записи (1, 3, 6, 12, 24).
    /// </summary>
    public int OffsetHours { get; set; }

    /// <summary>
    /// Включено ли напоминание с данным offset.
    /// </summary>
    public bool Enabled { get; set; }

    public AppUser AppUser { get; set; } = null!;
}
