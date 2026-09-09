using BlankoCRM.Core.Entities;
using BlankoCRM.Core.Interfaces;
using BlankoCRM.Infrastructure.Repositories;
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
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddScoped<IScheduleRepository, ScheduleRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();
        services.AddScoped<IRepository<Client>, EfRepository<Client>>();

        return services;
    }
}
