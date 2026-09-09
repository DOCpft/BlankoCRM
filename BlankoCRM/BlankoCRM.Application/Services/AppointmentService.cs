using BlankoCRM.Application.Abstract;
using BlankoCRM.Application.DTOs;
using BlankoCRM.Core.Entities;
using BlankoCRM.Core.Interfaces;
using BlankoCRM.Core.Exceptions;
using BlankoCRM.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Linq;

namespace BlankoCRM.Application.Services;

/// <summary>
/// Р РµР°Р»РёР·Р°С†РёСЏ СЃРµСЂРІРёСЃР° РґР»СЏ СЂР°Р±РѕС‚С‹ СЃ Р·Р°РїРёСЃСЏРјРё РЅР° РїСЂРёС‘Рј.
/// </summary>
public class AppointmentService : IAppointmentService
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IRepository<Client> _clientRepository;
    private readonly IRepository<Service> _serviceRepository;
    private readonly IRepository<Salon> _salonRepository;
    private readonly IOrganizationContext _organizationContext;

    public AppointmentService(
        IAppointmentRepository appointmentRepository,
        IRepository<Client> clientRepository,
        IRepository<Service> serviceRepository,
        IRepository<Salon> salonRepository,
        IOrganizationContext organizationContext)
    {
        _appointmentRepository = appointmentRepository;
        _clientRepository = clientRepository;
        _serviceRepository = serviceRepository;
        _salonRepository = salonRepository;
        _organizationContext = organizationContext;
    }

    public async Task<Appointment?> GetByIdAsync(Guid id)
    {
        return await _appointmentRepository.GetByIdWithDetailsAsync(id);
    }

    public async Task<List<Appointment>> GetForDayAsync(DateTime date)
    {
        return (await _appointmentRepository.GetForDayAsync(date)).ToList();
    }

    public async Task<List<Appointment>> GetAllAppointmentsAsync()
    {
        var appointments = await _appointmentRepository.GetAllWithDetailsAsync();
        return appointments.ToList();
    }

    public async Task<Appointment> CreateAsync(
        Guid clientId,
        List<ServiceInput> serviceInputs,
        DateTime startTime,
        string? notes = null,
        Guid? salonId = null)
    {
        // 1. РџСЂРѕРІРµСЂСЏРµРј РєР»РёРµРЅС‚Р°
        var client = await _clientRepository.GetByIdAsync(clientId)
            ?? throw new NotFoundException($"Client with ID {clientId} was not found.");

        if (salonId.HasValue && await _salonRepository.GetByIdAsync(salonId.Value) is null)
        {
            throw new NotFoundException($"Salon with ID {salonId.Value} was not found in the current organization.");
        }

        // 2. Р—Р°РіСЂСѓР¶Р°РµРј РІСЃРµ СѓСЃР»СѓРіРё РёР· Р‘Р” РѕРґРЅРёРј Р·Р°РїСЂРѕСЃРѕРј
        var serviceIds = serviceInputs.Select(si => si.ServiceId).Distinct().ToList();
        var services = (await _serviceRepository.FindAsync(service => serviceIds.Contains(service.Id)))
            .ToDictionary(service => service.Id);

        // 3. РџСЂРѕРІРµСЂСЏРµРј, С‡С‚Рѕ РІСЃРµ СѓСЃР»СѓРіРё РЅР°Р№РґРµРЅС‹
        var missing = serviceIds.Except(services.Keys).ToList();
        if (missing.Any())
            throw new NotFoundException($"Services with IDs {string.Join(", ", missing)} were not found.");

        // 4. РЎРѕР·РґР°С‘Рј СЃРїРёСЃРѕРє AppointmentService
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
                OrganizationId = _organizationContext.GetCurrentOrganizationId()!.Value,
                Price = price,
                DurationMinutes = duration,
                ServiceName = service.Name,
                Quantity = input.Quantity > 0 ? input.Quantity : 1
            });
        }

        // 5. Р Р°СЃСЃС‡РёС‚С‹РІР°РµРј РѕР±С‰СѓСЋ РґР»РёС‚РµР»СЊРЅРѕСЃС‚СЊ (СЃСѓРјРјР° РІСЃРµС… РґР»РёС‚РµР»СЊРЅРѕСЃС‚РµР№)
        var totalDuration = appointmentServices.Sum(s => s.DurationMinutes * s.Quantity);
        var end = startTime.AddMinutes(totalDuration);
        var organizationId = _organizationContext.GetCurrentOrganizationId()!.Value;

        if (await _appointmentRepository.HasOverlapAsync(startTime, end, organizationId, salonId))
        {
            throw new ConflictException($"Время записи {startTime:g} - {end:g} уже занято.");
        }

        // 6. РЎРѕР·РґР°С‘Рј Appointment
        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            SalonId = salonId,
            ClientId = client.Id,
            Client = client,
            StartTime = startTime,
            EndTime = end,
            Status = AppointmentStatus.Scheduled,
            Notes = notes,
            ReminderSent = false,
            CreatedAt = DateTime.UtcNow,
            AppointmentServices = appointmentServices
        };

        // 7. РЎРѕС…СЂР°РЅСЏРµРј (РґРѕСЃС‚Р°С‚РѕС‡РЅРѕ РґРѕР±Р°РІРёС‚СЊ С‚РѕР»СЊРєРѕ РєРѕСЂРЅРµРІСѓСЋ СЃСѓС‰РЅРѕСЃС‚СЊ)
        await _appointmentRepository.AddAsync(appointment);

        return appointment;
    }

    public async Task UpdateAsync(Guid id, DateTime? startTime, AppointmentStatus? status, string? notes)
    {
        var appointment = await _appointmentRepository.GetByIdWithDetailsAsync(id)
            ?? throw new NotFoundException($"Appointment with ID {id} was not found.");

        if (startTime.HasValue)
        {
            var duration = appointment.AppointmentServices.Sum(s => s.DurationMinutes * s.Quantity);
            var endTime = startTime.Value.AddMinutes(duration);
            if (await _appointmentRepository.HasOverlapAsync(
                startTime.Value,
                endTime,
                appointment.OrganizationId,
                appointment.SalonId,
                appointment.Id))
            {
                throw new ConflictException($"Время записи {startTime.Value:g} - {endTime:g} уже занято.");
            }

            appointment.StartTime = startTime.Value;
            appointment.EndTime = endTime;
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

        await _appointmentRepository.UpdateAsync(appointment);
    }

    public async Task DeleteAsync(Guid id)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Appointment with ID {id} was not found.");

        await _appointmentRepository.DeleteAsync(appointment);
    }

    public async Task CancelAsync(Guid id)
    {
        var appointment = await _appointmentRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Appointment with ID {id} was not found.");

        appointment.Status = AppointmentStatus.Cancelled;
        appointment.UpdatedAt = DateTime.UtcNow;

        await _appointmentRepository.UpdateAsync(appointment);
    }
}

