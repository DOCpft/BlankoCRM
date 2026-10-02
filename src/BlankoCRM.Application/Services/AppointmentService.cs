using BlankoCRM.Application.Abstract;
using BlankoCRM.Application.DTOs;
using BlankoCRM.Core.Entities;
using BlankoCRM.Core.Interfaces;
using BlankoCRM.Core.Exceptions;
using Microsoft.Extensions.Logging;
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
    private readonly IScheduleRepository _scheduleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrganizationContext _organizationContext;
    private readonly ILogger<AppointmentService> _logger;

    public AppointmentService(
        IAppointmentRepository appointmentRepository,
        IRepository<Client> clientRepository,
        IRepository<Service> serviceRepository,
        IRepository<Salon> salonRepository,
        IScheduleRepository scheduleRepository,
        IUnitOfWork unitOfWork,
        IOrganizationContext organizationContext,
        ILogger<AppointmentService> logger)
    {
        _appointmentRepository = appointmentRepository;
        _clientRepository = clientRepository;
        _serviceRepository = serviceRepository;
        _salonRepository = salonRepository;
        _scheduleRepository = scheduleRepository;
        _unitOfWork = unitOfWork;
        _organizationContext = organizationContext;
        _logger = logger;
    }

    public async Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _appointmentRepository.GetByIdWithDetailsAsync(id);
    }

    public async Task<List<Appointment>> GetForDayAsync(DateTime date, CancellationToken cancellationToken = default)
    {
        return (await _appointmentRepository.GetForDayAsync(date)).ToList();
    }

    public async Task<List<Appointment>> GetAllAppointmentsAsync(CancellationToken cancellationToken = default)
    {
        var appointments = await _appointmentRepository.GetAllWithDetailsAsync();
        return appointments.ToList();
    }

    public async Task<List<Appointment>> GetPendingAppointmentsAsync(CancellationToken cancellationToken = default)
    {
        var organizationId = _organizationContext.GetCurrentOrganizationId()
            ?? throw new UnauthorizedAccessException("Organization context is not initialized.");
        var appointments = await _appointmentRepository.GetActivePendingAsync(
            organizationId,
            DateTime.UtcNow,
            cancellationToken);
        return appointments.ToList();
    }

    public async Task<Appointment> CreateAsync(
        Guid clientId,
        List<ServiceInput> serviceInputs,
        DateTime startTime,
        string? notes = null,
        Guid? salonId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await CreateWithScheduleAsync(
            clientId,
            serviceInputs,
            startTime,
            notes,
            salonId,
            cancellationToken: cancellationToken);
        return result.Appointment;
    }

    public async Task<(Appointment Appointment, Schedule Schedule)> CreateWithScheduleAsync(
        Guid clientId,
        List<ServiceInput> serviceInputs,
        DateTime startTime,
        string? notes = null,
        Guid? salonId = null,
        DateTime? endTime = null,
        CancellationToken cancellationToken = default)
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
                Quantity = input.Quantity ?? 1
            });
        }

        // 5. Р Р°СЃСЃС‡РёС‚С‹РІР°РµРј РѕР±С‰СѓСЋ РґР»РёС‚РµР»СЊРЅРѕСЃС‚СЊ (СЃСѓРјРјР° РІСЃРµС… РґР»РёС‚РµР»СЊРЅРѕСЃС‚РµР№)
        var end = endTime ?? startTime.AddMinutes(
            appointmentServices.Sum(service => service.DurationMinutes * service.Quantity));
        if (end <= startTime)
        {
            throw new ValidationException("Время окончания должно быть позже времени начала.");
        }
        var organizationId = _organizationContext.GetCurrentOrganizationId()!.Value;

        if (await _appointmentRepository.HasOverlapAsync(
            startTime,
            end,
            organizationId,
            salonId,
            cancellationToken: cancellationToken))
        {
            _logger.LogWarning(
                "Appointment conflict: interval {Start}-{End}, organization {OrganizationId}",
                startTime, end, organizationId);
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
        var schedule = new Schedule
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            SalonId = salonId,
            Appointment = appointment,
            StartTime = startTime,
            EndTime = end,
            CreatedAt = DateTime.UtcNow
        };

        var ownsTransaction = !_unitOfWork.HasActiveTransaction;
        await using var transaction = ownsTransaction
            ? await _unitOfWork.BeginTransactionAsync(cancellationToken)
            : null;
        _unitOfWork.Add(appointment);
        _unitOfWork.Add(schedule);
        await _scheduleRepository.DeleteFreeSlotsByTimeAsync(
            startTime, end, organizationId, salonId, cancellationToken: cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        if (ownsTransaction)
        {
            await transaction!.CommitAsync(cancellationToken);
        }

        _logger.LogInformation(
            "Appointment {AppointmentId} created for client {ClientId} at {StartTime}",
            appointment.Id, appointment.ClientId, appointment.StartTime);

        return (appointment, schedule);
    }

    public async Task UpdateAsync(Guid id, UpdateAppointmentRequest request, CancellationToken cancellationToken = default)
    {
        var ownsTransaction = !_unitOfWork.HasActiveTransaction;
        await using var transaction = ownsTransaction
            ? await _unitOfWork.BeginTransactionAsync(cancellationToken)
            : null;
        var organizationId = _organizationContext.GetCurrentOrganizationId()
            ?? throw new UnauthorizedAccessException("Organization context is not initialized.");
        await _appointmentRepository.LockForUpdateAsync(organizationId, id, cancellationToken);
        var appointment = await _appointmentRepository.GetByIdWithDetailsAsync(id)
            ?? throw new NotFoundException($"Appointment with ID {id} was not found.");

        if (appointment.Status == AppointmentStatus.Pending)
        {
            throw new ConflictException("Ожидающую заявку нельзя редактировать. Используйте подтверждение, отклонение или отмену.");
        }

        if (request.ClientId.HasValue && request.ClientId.Value != appointment.ClientId)
        {
            appointment.Client = await _clientRepository.GetByIdAsync(request.ClientId.Value)
                ?? throw new NotFoundException($"Client with ID {request.ClientId.Value} was not found.");
            appointment.ClientId = request.ClientId.Value;
        }

        if (request.SalonId.HasValue && await _salonRepository.GetByIdAsync(request.SalonId.Value) is null)
        {
            throw new NotFoundException($"Salon with ID {request.SalonId.Value} was not found in the current organization.");
        }

        if (request.SalonId.HasValue)
        {
            appointment.SalonId = request.SalonId;
        }

        if (request.Services is not null)
        {
            var serviceIds = request.Services.Select(service => service.ServiceId).Distinct().ToList();
            var services = (await _serviceRepository.FindAsync(service => serviceIds.Contains(service.Id)))
                .ToDictionary(service => service.Id);
            var missing = serviceIds.Except(services.Keys).ToList();
            if (missing.Count > 0)
            {
                throw new NotFoundException($"Services with IDs {string.Join(", ", missing)} were not found.");
            }

            appointment.AppointmentServices.Clear();
            foreach (var input in request.Services)
            {
                var service = services[input.ServiceId];
                appointment.AppointmentServices.Add(new Core.Entities.AppointmentService
                {
                    Id = Guid.NewGuid(),
                    AppointmentId = appointment.Id,
                    ServiceId = service.Id,
                    OrganizationId = appointment.OrganizationId,
                    Price = input.Price ?? service.Price,
                    DurationMinutes = input.DurationMinutes ?? service.DurationMinutes,
                    ServiceName = service.Name,
                    Quantity = input.Quantity ?? 1
                });
            }
        }

        var newStartTime = request.StartTime ?? appointment.StartTime;
        var duration = appointment.AppointmentServices.Sum(service => service.DurationMinutes * service.Quantity);
        var endTime = newStartTime.AddMinutes(duration);
        var schedule = await _scheduleRepository.GetSlotByAppointmentIdAsync(appointment.Id);

        if (request.StartTime.HasValue || request.Services is not null || request.SalonId.HasValue)
        {
            if (await _appointmentRepository.HasOverlapAsync(
                newStartTime,
                endTime,
                appointment.OrganizationId,
                appointment.SalonId,
                appointment.Id,
                cancellationToken))
            {
                throw new ConflictException($"Время записи {newStartTime:g} - {endTime:g} уже занято.");
            }

            appointment.StartTime = newStartTime;
            appointment.EndTime = endTime;
            if (schedule is not null)
            {
                schedule.StartTime = newStartTime;
                schedule.EndTime = endTime;
                schedule.SalonId = appointment.SalonId;
                await _scheduleRepository.DeleteFreeSlotsByTimeAsync(
                    newStartTime,
                    endTime,
                    appointment.OrganizationId,
                    appointment.SalonId,
                    schedule.Id,
                    cancellationToken);
            }
        }

        if (request.Notes is not null)
        {
            appointment.Notes = request.Notes;
        }

        appointment.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        if (ownsTransaction)
        {
            await transaction!.CommitAsync(cancellationToken);
        }
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var ownsTransaction = !_unitOfWork.HasActiveTransaction;
        await using var transaction = ownsTransaction
            ? await _unitOfWork.BeginTransactionAsync(cancellationToken)
            : null;
        var organizationId = _organizationContext.GetCurrentOrganizationId()
            ?? throw new UnauthorizedAccessException("Organization context is not initialized.");
        await _appointmentRepository.LockForUpdateAsync(organizationId, id, cancellationToken);
        var appointment = await _appointmentRepository.GetByIdWithDetailsAsync(id)
            ?? throw new NotFoundException($"Appointment with ID {id} was not found.");

        if (appointment.Status == AppointmentStatus.Pending)
        {
            throw new ConflictException("Ожидающую заявку нельзя удалить.");
        }

        var schedule = await _scheduleRepository.GetSlotByAppointmentIdAsync(id);
        if (schedule is not null)
        {
            schedule.AppointmentId = null;
            schedule.Appointment = null;
        }
        appointment.Status = AppointmentStatus.Archived;
        appointment.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        if (ownsTransaction)
        {
            await transaction!.CommitAsync(cancellationToken);
        }

        _logger.LogInformation("Appointment {AppointmentId} archived", appointment.Id);
    }

    public async Task ConfirmAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var ownsTransaction = !_unitOfWork.HasActiveTransaction;
        await using var transaction = ownsTransaction
            ? await _unitOfWork.BeginTransactionAsync(cancellationToken)
            : null;

        try
        {
            var organizationId = _organizationContext.GetCurrentOrganizationId()
                ?? throw new UnauthorizedAccessException("Organization context is not initialized.");
            await _appointmentRepository.LockForUpdateAsync(organizationId, id, cancellationToken);
            var appointment = await _appointmentRepository.GetByIdWithDetailsAsync(id, cancellationToken)
                ?? throw new NotFoundException($"Appointment with ID {id} was not found.");

            if (appointment.Status != AppointmentStatus.Pending)
            {
                if (appointment.Status == AppointmentStatus.Scheduled)
                {
                    throw new ConflictException("Запись уже подтверждена");
                }

                throw new ConflictException("Подтвердить можно только ожидающую заявку.");
            }

            if (!appointment.ExpiresAt.HasValue || appointment.ExpiresAt.Value <= DateTime.UtcNow)
            {
                throw new ConflictException("Срок действия заявки истёк.");
            }

            if (await _appointmentRepository.HasOverlapAsync(
                appointment.StartTime,
                appointment.EndTime,
                organizationId,
                appointment.SalonId,
                appointment.Id,
                cancellationToken))
            {
                throw new ConflictException("На это время уже есть подтверждённая запись.");
            }

            if (await _scheduleRepository.GetSlotByAppointmentIdAsync(appointment.Id, cancellationToken) is not null)
            {
                throw new ConflictException("Для заявки уже существует запись в расписании.");
            }

            var now = DateTime.UtcNow;
            appointment.Status = AppointmentStatus.Scheduled;
            appointment.UpdatedAt = now;
            var schedule = new Schedule
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                SalonId = appointment.SalonId,
                AppointmentId = appointment.Id,
                Appointment = appointment,
                StartTime = appointment.StartTime,
                EndTime = appointment.EndTime,
                CreatedAt = now
            };
            appointment.Schedule = schedule;

            _unitOfWork.Add(schedule);
            await _scheduleRepository.DeleteFreeSlotsByTimeAsync(
                appointment.StartTime,
                appointment.EndTime,
                organizationId,
                appointment.SalonId,
                cancellationToken: cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            if (ownsTransaction)
            {
                await transaction!.CommitAsync(cancellationToken);
            }

            _logger.LogInformation("Pending appointment {AppointmentId} confirmed", appointment.Id);
        }
        catch
        {
            if (ownsTransaction && _unitOfWork.HasActiveTransaction)
            {
                await transaction!.RollbackAsync(cancellationToken);
            }

            throw;
        }
    }

    public Task RejectAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return TransitionStatusAsync(
            id,
            AppointmentStatus.Rejected,
            [AppointmentStatus.Pending],
            releaseSchedule: false,
            cancellationToken);
    }

    public Task CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return TransitionStatusAsync(
            id,
            AppointmentStatus.Cancelled,
            [AppointmentStatus.Pending, AppointmentStatus.Scheduled],
            releaseSchedule: true,
            cancellationToken);
    }

    public Task CompleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return TransitionStatusAsync(
            id,
            AppointmentStatus.Completed,
            [AppointmentStatus.Scheduled],
            releaseSchedule: false,
            cancellationToken);
    }

    public Task MarkNoShowAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return TransitionStatusAsync(
            id,
            AppointmentStatus.NoShow,
            [AppointmentStatus.Scheduled],
            releaseSchedule: false,
            cancellationToken);
    }

    private async Task TransitionStatusAsync(
        Guid id,
        AppointmentStatus targetStatus,
        IReadOnlyCollection<AppointmentStatus> allowedSourceStatuses,
        bool releaseSchedule,
        CancellationToken cancellationToken)
    {
        var ownsTransaction = !_unitOfWork.HasActiveTransaction;
        await using var transaction = ownsTransaction
            ? await _unitOfWork.BeginTransactionAsync(cancellationToken)
            : null;

        try
        {
            var organizationId = _organizationContext.GetCurrentOrganizationId()
                ?? throw new UnauthorizedAccessException("Organization context is not initialized.");
            await _appointmentRepository.LockForUpdateAsync(organizationId, id, cancellationToken);
            var appointment = await _appointmentRepository.GetByIdWithDetailsAsync(id, cancellationToken)
                ?? throw new NotFoundException($"Appointment with ID {id} was not found.");

            if (!allowedSourceStatuses.Contains(appointment.Status))
            {
                throw new ConflictException($"Недопустимый переход статуса {appointment.Status} -> {targetStatus}.");
            }

            if (targetStatus == AppointmentStatus.Cancelled
                && appointment.Status == AppointmentStatus.Scheduled
                && releaseSchedule)
            {
                var schedule = await _scheduleRepository.GetSlotByAppointmentIdAsync(id, cancellationToken);
                if (schedule is not null)
                {
                    schedule.AppointmentId = null;
                    schedule.Appointment = null;
                }
            }

            appointment.Status = targetStatus;
            appointment.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            if (ownsTransaction)
            {
                await transaction!.CommitAsync(cancellationToken);
            }

            _logger.LogInformation(
                "Appointment {AppointmentId} status changed to {Status}",
                appointment.Id,
                targetStatus);
        }
        catch
        {
            if (ownsTransaction && _unitOfWork.HasActiveTransaction)
            {
                await transaction!.RollbackAsync(cancellationToken);
            }

            throw;
        }
    }
}

