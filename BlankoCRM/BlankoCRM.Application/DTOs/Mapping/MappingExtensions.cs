using BlankoCRM.Application.DTOs;
using BlankoCRM.Core.Entities;

namespace BlankoCRM.Application.DTOs.Mapping
{
    /// <summary>
    /// Extension-методы для маппинга Entity -> DTO.
    /// Централизованное место для всех преобразований.
    /// При разрастании проекта рекомендуется заменить на AutoMapper.
    /// </summary>
    public static class MappingExtensions
    {
        public static ClientDto ToDto(this Client client)
        {
            return new ClientDto(
                client.Id,
                client.Name,
                client.Phone,
                client.Telegram,
                client.BirthDate,
                client.Notes);
        }

        public static ServiceDto ToDto(this Service service)
        {
            return new ServiceDto(
                service.Id,
                service.Name,
                service.DurationMinutes,
                service.Price,
                service.Description);
        }

        public static AppointmentDto ToDto(this Appointment appointment)
        {
            var firstService = appointment.AppointmentServices?.FirstOrDefault();

            return new AppointmentDto(
                appointment.Id,
                appointment.ClientId,
                appointment.Client?.Name ?? string.Empty,
                firstService?.ServiceId,
                firstService?.ServiceName,
                appointment.StartTime,
                appointment.EndTime,
                appointment.Status,
                appointment.Notes,
                appointment.TotalPrice,
                (int)appointment.TotalDuration);
        }

        public static ScheduleDto ToDto(this Schedule schedule)
        {
            var processedAppointment = schedule.Appointment?.ToDto();
            return new ScheduleDto(
                schedule.Id,
                schedule.SlotTime,
                processedAppointment
                );
        }
    }
}
