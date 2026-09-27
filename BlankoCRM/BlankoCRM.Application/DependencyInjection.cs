using BlankoCRM.Application.Abstract;
using BlankoCRM.Application.Services;
using BlankoCRM.Application.DTOs.Mapping;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using System.Reflection;
using BlankoCRM.Core.Interfaces;

namespace BlankoCRM.Application;

/// <summary>
/// Регистрация сервисов слоя Application в DI-контейнере.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddAutoMapper(configuration =>
            configuration.AddMaps(typeof(AutoMapperProfile).Assembly));
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IPublicClientResolver, PublicClientResolver>();
        services.AddScoped<IServicesService, ServicesService>();
        services.AddScoped<IAppointmentService, AppointmentService>();
        services.AddScoped<IAppointmentExpirationService, AppointmentExpirationService>();
        services.AddScoped<IPublicAppointmentService, PublicAppointmentService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IScheduleService, ScheduleService>();
        services.AddScoped<IVkSignatureValidator, VkSignatureValidator>();
        services.AddScoped<IVkMiniAppAuthenticator, VkMiniAppAuthenticator>();

        return services;
    }
}

