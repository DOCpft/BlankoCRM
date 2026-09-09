using BlankoCRM.Application.Abstract;
using BlankoCRM.Application.Services;
using BlankoCRM.Application.DTOs.Mapping;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using System.Reflection;

namespace BlankoCRM.Application;

/// <summary>
/// Р РµРіРёСЃС‚СЂР°С†РёСЏ СЃРµСЂРІРёСЃРѕРІ СЃР»РѕСЏ Application РІ DI-РєРѕРЅС‚РµР№РЅРµСЂРµ.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(configuration =>
            configuration.AddMaps(typeof(AutoMapperProfile).Assembly));
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IServicesService, ServicesService>();
        services.AddScoped<IAppointmentService, AppointmentService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IScheduleService, ScheduleService>();

        return services;
    }
}

