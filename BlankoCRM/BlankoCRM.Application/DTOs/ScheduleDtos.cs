namespace BlankoCRM.Application.DTOs
{
    /// <summary>
    /// DTO для отображения слотов по расписанию мастера
    /// </summary>
    /// <param name="Id"></param>
    /// <param name="SlotTime"></param>
    /// <param name="Appointment"></param>
    public record ScheduleDto(
        Guid Id,
        DateTime SlotTime,
        AppointmentDto? Appointment
    );

    public record CreateSlotRequest(
        DateTime slotTime,
        Guid? AppointmentId
        );

    public record CreateManySlotsRequest(
        List<Tuple<DateTime, Guid?>> tpSlotsList
        );

    public record UpdateSlotRequest(
        DateTime? slotTime,
        Guid? AppointmentId
        );
}
