using BlankoCRM.Core.Interfaces;

namespace BlankoCRM.Core.Entities;

/// <summary>
/// Журнал отправленных напоминаний. Предотвращает повторную отправку напоминания
/// для одной пары (запись, offset).
/// </summary>
public sealed class AppointmentReminderLog : IOrganizationEntity
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid AppointmentId { get; set; }
    public int OffsetHours { get; set; }
    public DateTime SentAt { get; set; }

    public Appointment Appointment { get; set; } = null!;
}
