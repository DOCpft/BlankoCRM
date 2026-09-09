using AutoMapper;
using BlankoCRM.Application.DTOs;
using BlankoCRM.Core.Entities;

namespace BlankoCRM.Application.DTOs.Mapping;

/// <summary>
/// Профиль преобразований между доменными сущностями и DTO.
/// </summary>
public sealed class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<Client, ClientDto>();
        CreateMap<Service, ServiceDto>();
        CreateMap<AppointmentService, AppointmentServiceDto>();
        CreateMap<Appointment, AppointmentDto>()
            .ForCtorParam(nameof(AppointmentDto.ClientName), options => options.MapFrom(appointment => appointment.Client.Name))
            .ForCtorParam(nameof(AppointmentDto.Services), options => options.MapFrom(appointment => appointment.AppointmentServices))
            .ForCtorParam(nameof(AppointmentDto.TotalPrice), options => options.MapFrom(appointment => appointment.TotalPrice))
            .ForCtorParam(nameof(AppointmentDto.TotalDurationMinutes), options => options.MapFrom(appointment => (int)appointment.TotalDuration));
        CreateMap<Schedule, ScheduleDto>();
    }
}
