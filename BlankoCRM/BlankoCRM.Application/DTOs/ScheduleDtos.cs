using BlankoCRM.Core.Entities;

namespace BlankoCRM.Application.DTOs
{
    /// <summary>
    /// DTO для отображения слота расписания.
    /// </summary>
    /// <param name="Id">Идентификатор слота.</param>
    /// <param name="StartTime">Время начала слота.</param>
    /// <param name="EndTime">Время окончания слота.</param>
    /// <param name="Appointment">Связанная запись на приём (для занятых слотов).</param>
    public record ScheduleDto(
        Guid Id,
        DateTime StartTime,
        DateTime EndTime,
        AppointmentDto? Appointment
    );

    /// <summary>
    /// Запрос на создание свободного слота.
    /// </summary>
    /// <param name="StartTime">Время начала слота.</param>
    /// <param name="EndTime">Время окончания слота.</param>
    /// <param name="SalonId">Идентификатор салона (необязательно).</param>
    public record CreateFreeSlotRequest(
        DateTime StartTime,
        DateTime EndTime,
        Guid? SalonId = null
    );

    /// <summary>
    /// Запрос на создание занятого слота.
    /// </summary>
    /// <param name="StartTime">Время начала слота.</param>
    /// <param name="EndTime">Время окончания слота (необязательно).</param>
    /// <param name="AppointmentData">Данные связанной записи на приём без времени.</param>
    public record CreateOccupiedSlotRequest(
        DateTime StartTime,
        CreateOccupiedSlotAppointmentData AppointmentData,
        DateTime? EndTime = null
    );

    public record CreateOccupiedSlotAppointmentData(
        Guid ClientId,
        List<ServiceInput> Services,
        string? Notes = null,
        Guid? SalonId = null
    );

    /// <summary>
    /// Запрос на создание нескольких слотов.
    /// </summary>
    /// <param name="Slots">Список свободных слотов для создания.</param>
    public record CreateManySlotsRequest(
        List<CreateFreeSlotRequest> Slots
        );

    /// <summary>
    /// Запрос на обновление слота.
    /// </summary>
    /// <param name="StartTime">Новое время начала слота (необязательно).</param>
    /// <param name="EndTime">Новое время окончания слота (необязательно).</param>
    /// <param name="AppointmentData">Данные связанной записи (необязательно).</param>
    public record UpdateSlotRequest(
        DateTime? StartTime,
        DateTime? EndTime,
        UpdateSlotAppointmentData? AppointmentData = null
        );

    /// <summary>
    /// Данные записи на приём при обновлении слота.
    /// </summary>
    /// <param name="ClientId">Идентификатор клиента.</param>
    /// <param name="Services">Список услуг.</param>
    /// <param name="Status">Статус записи.</param>
    /// <param name="Notes">Заметки.</param>
    /// <param name="SalonId">Идентификатор салона.</param>
    public record UpdateSlotAppointmentData(
        Guid? ClientId,
        List<ServiceInput>? Services,
        AppointmentStatus? Status,
        string? Notes,
        Guid? SalonId = null
    );
}