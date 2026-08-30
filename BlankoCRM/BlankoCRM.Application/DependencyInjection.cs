using BlankoCRM.Application.Abstract;
using BlankoCRM.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BlankoCRM.Application;

/// <summary>
/// Регистрация сервисов слоя Application в DI-контейнере.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IServicesService, ServicesService>();
        services.AddScoped<IAppointmentService, AppointmentService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IScheduleService, ScheduleService>();

        return services;
    }
}
