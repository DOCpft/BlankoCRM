using BlankoCRM.Application.Abstract;
using BlankoCRM.Application.DTOs;
using BlankoCRM.Core.Entities;
using BlankoCRM.Infrastructure;
using BlankoCRM.Infrastructure.Data;
using BlankoCRM.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Linq;

namespace BlankoCRM.Application.Services;

/// <summary>
/// Реализация сервиса для работы с записями на приём.
/// </summary>
public class AppointmentService : IAppointmentService
{
    private readonly AppDbContext _dbContext;

    public AppointmentService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Appointment?> GetByIdAsync(Guid id)
    {
        return await _dbContext.Appointments
            .Include(a => a.Client)
            .Include(a => a.AppointmentServices)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<List<Appointment>> GetForDayAsync(DateTime date)
    {
        var dayStart = date.Date;
        var dayEnd = dayStart.AddDays(1);

        return await _dbContext.Appointments
            .Include(a => a.Client)
            .Include(a => a.AppointmentServices)
            .Where(a => a.StartTime >= dayStart && a.StartTime < dayEnd)
            .OrderBy(a => a.StartTime)
            .ToListAsync();
    }

    public async Task<List<Appointment>> GetAllAppointmentsAsync()
    {
        var appointments = await _dbContext.Appointments
                    .Include(a => a.Client)
                    .Include(a => a.AppointmentServices)
                    .OrderBy(a => a.StartTime).ToListAsync();
        if (appointments.Count == 0) throw new KeyNotFoundException("У мастера отсутствуют записи");
        return appointments;
    }

    public async Task<Appointment> CreateAsync(
        Guid clientId,
        List<ServiceInput> serviceInputs,
        DateTime startTime,
        string? notes = null)
    {
        // 1. Проверяем клиента
        var client = await _dbContext.Clients.FindOrThrowAsync(clientId);

        // 2. Загружаем все услуги из БД одним запросом
        var serviceIds = serviceInputs.Select(si => si.ServiceId).Distinct().ToList();
        var services = await _dbContext.Services
            .Where(s => serviceIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id);

        // 3. Проверяем, что все услуги найдены
        var missing = serviceIds.Except(services.Keys).ToList();
        if (missing.Any())
            throw new KeyNotFoundException($"Услуги с ID {string.Join(", ", missing)} не найдены");

        // 4. Создаём список AppointmentService
        var appointmentServices = new List<Core.Entities.AppointmentService>();

        foreach (var input in serviceInputs)
        {
            var service = services[input.ServiceId];
            var duration = input.DurationMinutes ?? service.DurationMinutes;
            var price = input.Price ?? service.Price;

            appointmentServices.Add(new Core.Entities.AppointmentService
            {
                Id = Guid.NewGuid(),
                ServiceId = service.Id,
                TenantId = TenantContext.CurrentTenantId!.Value,
                Price = price,
                DurationMinutes = duration,
                ServiceName = service.Name,
                Quantity = input.Quantity > 0 ? input.Quantity : 1
            });
        }

        // 5. Рассчитываем общую длительность (сумма всех длительностей)
        var totalDuration = appointmentServices.Sum(s => s.DurationMinutes * s.Quantity);
        var end = startTime.AddMinutes(totalDuration);

        // 6. Создаём Appointment
        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            TenantId = TenantContext.CurrentTenantId!.Value,
            ClientId = client.Id,
            StartTime = startTime,
            EndTime = end,
            Status = AppointmentStatus.Scheduled,
            Notes = notes,
            ReminderSent = false,
            CreatedAt = DateTime.UtcNow,
            AppointmentServices = appointmentServices
        };

        // 7. Сохраняем (достаточно добавить только корневую сущность)
        _dbContext.Appointments.Add(appointment);
        await _dbContext.SaveChangesAsync();

        // 8. Возвращаем полный объект с загруженными связями
        return (await GetByIdAsync(appointment.Id))!;
    }

    public async Task UpdateAsync(Guid id, DateTime? startTime, AppointmentStatus? status, string? notes)
    {
        var appointment = await _dbContext.Appointments
            .Include(a => a.AppointmentServices)
            .FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new KeyNotFoundException($"Запись с ID {id} не найдена");

        if (startTime.HasValue)
        {
            var duration = appointment.AppointmentServices.Sum(s => s.DurationMinutes);
            appointment.StartTime = startTime.Value;
            appointment.EndTime = appointment.StartTime.AddMinutes(duration);
        }

        if (status.HasValue)
        {
            appointment.Status = status.Value;
        }

        if (notes is not null)
        {
            appointment.Notes = notes;
        }

        appointment.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var appointment = await _dbContext.Appointments.FindOrThrowAsync(id);

        _dbContext.Appointments.Remove(appointment);
        await _dbContext.SaveChangesAsync();
    }

    public async Task CancelAsync(Guid id)
    {
        var appointment = await _dbContext.Appointments.FindOrThrowAsync(id);

        appointment.Status = AppointmentStatus.Cancelled;
        appointment.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
    }
}
