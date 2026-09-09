using BlankoCRM.Core.Entities;

namespace BlankoCRM.Application.DTOs
{
    /// <summary>
    /// DTO для отображения слотов по расписанию мастера
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="StartTime"></param>
    /// <param name="EndTime"></param>
    /// <param name="Appointment"></param>
    public record ScheduleDto(
        Guid Id,
        DateTime StartTime,
        DateTime EndTime,
        AppointmentDto? Appointment
    );

    public record CreateFreeSlotRequest(
        DateTime StartTime,
        DateTime EndTime,
        Guid? SalonId = null
    );

    public record CreateOccupiedSlotRequest(
        DateTime StartTime,
        DateTime EndTime,
        CreateAppointmentRequest AppointmentData
    );

    public record CreateManySlotsRequest(
        List<CreateFreeSlotRequest> Slots
        );

    public record UpdateSlotRequest(
        DateTime? StartTime,
        DateTime? EndTime,
        UpdateSlotAppointmentData? AppointmentData = null
        );

    public record UpdateSlotAppointmentData(
        Guid? ClientId,
        List<ServiceInput>? Services,
        AppointmentStatus? Status,
        string? Notes,
        Guid? SalonId = null
    );
}

