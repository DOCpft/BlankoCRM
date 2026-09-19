using System.ComponentModel.DataAnnotations;
using BlankoCRM.Core.Entities;

namespace BlankoCRM.Application.DTOs
{
    /// <summary>
    /// Услуга в составе записи на приём.
    /// </summary>
    /// <param name="ServiceId">Идентификатор услуги.</param>
    /// <param name="ServiceName">Название услуги.</param>
    /// <param name="Price">Цена услуги.</param>
    /// <param name="DurationMinutes">Длительность услуги в минутах.</param>
    /// <param name="Quantity">Количество услуг в записи.</param>
    public record AppointmentServiceDto(
        Guid ServiceId,
        string? ServiceName,
        decimal Price,
        int DurationMinutes,
        int Quantity);

    /// <summary>
    /// DTO для отображения записи на приём.
    /// </summary>
    /// <param name="Id">Идентификатор записи.</param>
    /// <param name="ClientId">Идентификатор клиента.</param>
    /// <param name="ClientName">Имя клиента.</param>
    /// <param name="Services">Список услуг в записи.</param>
    /// <param name="StartTime">Время начала записи.</param>
    /// <param name="EndTime">Время окончания записи.</param>
    /// <param name="Status">Статус записи.</param>
    /// <param name="Notes">Заметки к записи.</param>
    /// <param name="TotalPrice">Итоговая стоимость записи.</param>
    /// <param name="TotalDurationMinutes">Итоговая длительность записи в минутах.</param>
    public record AppointmentDto(
        Guid Id,
        Guid ClientId,
        string ClientName,
        List<AppointmentServiceDto> Services,
        DateTime StartTime,
        DateTime EndTime,
        AppointmentStatus Status,
        string? Notes,
        decimal TotalPrice,
        int TotalDurationMinutes);

    /// <summary>
    /// Запрос на создание новой записи.
    /// </summary>
    /// <param name="ClientId">Идентификатор клиента. Обязателен.</param>
    /// <param name="Services">Список услуг. Должна быть указана хотя бы одна услуга.</param>
    /// <param name="StartTime">Время начала записи. Обязательно.</param>
    /// <param name="Notes">Заметки (не более 500 символов).</param>
    /// <param name="SalonId">Идентификатор салона (необязательно).</param>
    public record CreateAppointmentRequest(
        [Required(ErrorMessage = "ID клиента обязателен")]
        Guid ClientId,

        [Required(ErrorMessage = "Необходимо указать хотя бы одну услугу")]
        [MinLength(1, ErrorMessage = "Должна быть указана хотя бы одна услуга")]
        List<ServiceInput> Services,

        [Required(ErrorMessage = "Время начала обязательно")]
        DateTime StartTime,

        [MaxLength(500, ErrorMessage = "Заметки не могут быть длиннее 500 символов")]
        string? Notes,

        Guid? SalonId = null
    );

    /// <summary>
    /// Входные данные по одной услуге в записи.
    /// </summary>
    /// <param name="ServiceId">Идентификатор услуги.</param>
    /// <param name="DurationMinutes">Переопределённая длительность в минутах (необязательно).</param>
    /// <param name="Price">Переопределённая цена (необязательно).</param>
    /// <param name="Quantity">Количество (необязательно, по умолчанию 1).</param>
    public record ServiceInput(
        Guid ServiceId,
        int? DurationMinutes = null,
        decimal? Price = null,
        int? Quantity = null
    );

    /// <summary>
    /// Запрос на обновление существующей записи.
    /// </summary>
    /// <param name="ClientId">Новый идентификатор клиента (необязательно).</param>
    /// <param name="Services">Новый список услуг (необязательно).</param>
    /// <param name="StartTime">Новое время начала (необязательно).</param>
    /// <param name="EndTime">Новое время окончания (необязательно).</param>
    /// <param name="Status">Новый статус (необязательно).</param>
    /// <param name="Notes">Новые заметки (не более 500 символов).</param>
    /// <param name="SalonId">Идентификатор салона (необязательно).</param>
    public record UpdateAppointmentRequest(
        Guid? ClientId,
        List<ServiceInput>? Services,
        DateTime? StartTime,
        DateTime? EndTime,
        AppointmentStatus? Status,

        [MaxLength(500, ErrorMessage = "Заметки не могут быть длиннее 500 символов")]
        string? Notes,
        Guid? SalonId = null);

    /// <summary>
    /// Запрос на изменение статуса записи.
    /// </summary>
    /// <param name="Status">Новый статус записи.</param>
    public record UpdateAppointmentStatusRequest(AppointmentStatus Status);
}