using System.ComponentModel.DataAnnotations;
using BlankoCRM.Core.Entities;

namespace BlankoCRM.Application.DTOs
{
    /// <summary>
    /// DTO для отображения записи на приём.
    /// </summary>
    public record AppointmentDto(
        Guid Id,
        Guid ClientId,
        string ClientName,
        Guid? ServiceId,
        string? ServiceName,
        DateTime StartTime,
        DateTime EndTime,
        AppointmentStatus Status,
        string? Notes,
        decimal TotalPrice,
        int TotalDurationMinutes);

    /// <summary>
    /// Запрос на создание новой записи.
    /// </summary>
    public record CreateAppointmentRequest(
        [Required(ErrorMessage = "ID клиента обязателен")]
        Guid ClientId,

        [Required(ErrorMessage = "Необходимо указать хотя бы одну услугу")]
        [MinLength(1, ErrorMessage = "Должна быть указана хотя бы одна услуга")]
        List<ServiceInput> Services,   // ← список с индивидуальными параметрами

        [Required(ErrorMessage = "Время начала обязательно")]
        DateTime StartTime,

        [MaxLength(500, ErrorMessage = "Заметки не могут быть длиннее 500 символов")]
        string? Notes
    );

    public record ServiceInput(
        Guid ServiceId,
        int? DurationMinutes = null,
        decimal? Price = null,
        int Quantity = 1
    );

    /// <summary>
    /// Запрос на обновление существующей записи.
    /// </summary>
    public record UpdateAppointmentRequest(
        DateTime? StartTime,
        AppointmentStatus? Status,

        [MaxLength(500, ErrorMessage = "Заметки не могут быть длиннее 500 символов")]
        string? Notes);
}
