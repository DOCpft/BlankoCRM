using BlankoCRM.Core.Entities;
using BlankoCRM.Core.Interfaces;
using BlankoCRM.Infrastructure.Repositories;
using BlankoCRM.Infrastructure.Data;
using BlankoCRM.Infrastructure.ExternalServices;
using Microsoft.Extensions.DependencyInjection;

namespace BlankoCRM.Infrastructure;

/// <summary>
/// Регистрация инфраструктурных компонентов.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Регистрирует репозитории EF Core.
    /// </summary>
    public static IServiceCollection AddInfrastructureRepositories(this IServiceCollection services)
    {
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IScheduleRepository, ScheduleRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IMasterReminderSettingRepository, MasterReminderSettingRepository>();
        services.AddScoped<IAppointmentReminderLogRepository, AppointmentReminderLogRepository>();

        return services;
    }

    /// <summary>
    /// Регистрирует внешние HTTP-клиенты.
    /// </summary>
    public static IServiceCollection AddInfrastructureExternalServices(this IServiceCollection services)
    {
        services.AddHttpClient<ITelegramBotClient, TelegramBotClient>(client =>
        {
            client.BaseAddress = new Uri("https://api.telegram.org");
        });

        return services;
    }
}
