using Microsoft.AspNetCore.Identity;

namespace BlankoCRM.Core.Entities;

/// <summary>
/// Пользователь системы на базе ASP.NET Core Identity.
/// </summary>
public class AppUser : IdentityUser<Guid>
{
    public Guid OrganizationId { get; set; }
    public Guid? SalonId { get; set; }
    public string Name { get; set; } = string.Empty;
    public long? VkUserId { get; set; }

    /// <summary>
    /// Идентификатор чата Telegram, к которому привязан мастер.
    /// </summary>
    public long? TelegramChatId { get; set; }

    /// <summary>
    /// Включены ли Telegram-уведомления для мастера.
    /// </summary>
    public bool TelegramNotificationsEnabled { get; set; } = true;

    /// <summary>
    /// Одноразовый код для привязки Telegram-аккаунта.
    /// </summary>
    public string? TelegramLinkCode { get; set; }

    /// <summary>
    /// Срок действия кода привязки Telegram-аккаунта.
    /// </summary>
    public DateTime? TelegramLinkCodeExpiresAt { get; set; }

    /// <summary>
    /// IANA-идентификатор часового пояса мастера (например, "Europe/Moscow").
    /// </summary>
    public string TimeZone { get; set; } = "Europe/Moscow";

    public Organization Organization { get; set; } = null!;
    public Salon? Salon { get; set; }
    public MasterProfile? MasterProfile { get; set; }
}
