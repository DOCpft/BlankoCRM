using BlankoCRM.Application.Abstract;
using BlankoCRM.Application.DTOs;
using BlankoCRM.Core.Entities;
using BlankoCRM.Core.Exceptions;
using BlankoCRM.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace BlankoCRM.Application.Services;

public class ScheduleService : IScheduleService
{
    private readonly IScheduleRepository _scheduleRepository;
    private readonly IAppointmentService _appointmentService;
    private readonly IOrganizationContext _organizationContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ScheduleService> _logger;

    public ScheduleService(
        IScheduleRepository scheduleRepository,
        IAppointmentService appointmentService,
        IOrganizationContext organizationContext,
        IUnitOfWork unitOfWork,
        ILogger<ScheduleService> logger)
    {
        _scheduleRepository = scheduleRepository;
        _appointmentService = appointmentService;
        _organizationContext = organizationContext;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Schedule> AddFreeSlotAsync(CreateFreeSlotRequest request, CancellationToken cancellationToken = default)
    {
        var organizationId = GetOrganizationId();
        if (await _scheduleRepository.HasOverlapAsync(
            request.StartTime, request.EndTime, organizationId, request.SalonId))
        {
            _logger.LogWarning(
                "Free slot conflict: interval {Start}-{End}, organization {OrganizationId}",
                request.StartTime, request.EndTime, organizationId);
            throw new ConflictException("Указанный интервал уже занят другим слотом.");
        }

        var slot = new Schedule
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            SalonId = request.SalonId,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            CreatedAt = DateTime.UtcNow
        };

        return await _scheduleRepository.AddAsync(slot);
    }

    public async Task<Schedule> AddOccupiedSlotAsync(CreateOccupiedSlotRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _appointmentService.CreateWithScheduleAsync(
            request.AppointmentData.ClientId,
            request.AppointmentData.Services,
            request.StartTime,
            request.AppointmentData.Notes,
            request.AppointmentData.SalonId,
            cancellationToken);

        return result.Schedule;
    }

    public async Task<List<Schedule>> AddSlotsAsync(List<CreateFreeSlotRequest> addedSlots, CancellationToken cancellationToken = default)
    {
        var slots = new List<Schedule>();
        foreach (var request in addedSlots)
        {
            slots.Add(await AddFreeSlotAsync(request, cancellationToken));
        }

        return slots;
    }

    public async Task<List<Schedule>> GetScheduleByPeriodAsync(DateTime dateFrom, DateTime dateTo, CancellationToken cancellationToken = default)
    {
        return (await _scheduleRepository.GetByPeriodAsync(dateFrom, dateTo, cancellationToken)).ToList();
    }

    public async Task<Schedule> GetSlotByIdAsync(Guid slotId, CancellationToken cancellationToken = default)
    {
        return await _scheduleRepository.GetByIdWithAppointmentAsync(slotId, cancellationToken)
            ?? throw new NotFoundException($"Schedule with ID {slotId} was not found.");
    }

    public async Task<bool> RemoveSlotAsync(Guid selectedSlot, CancellationToken cancellationToken = default)
    {
        var slot = await GetSlotByIdAsync(selectedSlot, cancellationToken);
        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        if (slot.Appointment is not null)
        {
            slot.Appointment.Status = AppointmentStatus.Archived;
            slot.Appointment.UpdatedAt = DateTime.UtcNow;
            slot.AppointmentId = null;
            slot.Appointment = null;
        }

        _unitOfWork.Remove(slot);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        _logger.LogInformation("Schedule slot {SlotId} removed", slot.Id);
        return true;
    }

    public async Task<int> RemoveSlotsAsync(List<Guid> selectedSlots, CancellationToken cancellationToken = default)
    {
        var slots = new List<Schedule>();
        foreach (var selectedSlot in selectedSlots)
        {
            var slot = await _scheduleRepository.GetByIdWithAppointmentAsync(selectedSlot, cancellationToken);
            if (slot is not null)
            {
                slots.Add(slot);
            }
        }

        if (slots.Count == 0)
        {
            return 0;
        }

        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        foreach (var slot in slots)
        {
            if (slot.Appointment is not null)
            {
                slot.Appointment.Status = AppointmentStatus.Archived;
                slot.Appointment.UpdatedAt = DateTime.UtcNow;
                slot.AppointmentId = null;
                slot.Appointment = null;
            }

            _unitOfWork.Remove(slot);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        _logger.LogInformation(
            "Schedule slots removed: requested {RequestedCount}, removed {RemovedCount}",
            selectedSlots.Count, slots.Count);
        return slots.Count;
    }

    public async Task UpdateSlotAsync(Guid id, UpdateSlotRequest request, CancellationToken cancellationToken = default)
    {
        var slot = await GetSlotByIdAsync(id, cancellationToken);
        var organizationId = GetOrganizationId();
        var startTime = request.StartTime ?? slot.StartTime;
        var endTime = request.EndTime ?? slot.EndTime;

        if (request.StartTime.HasValue || request.EndTime.HasValue)
        {
            if (slot.Appointment is null)
            {
                if (await _scheduleRepository.HasOverlapAsync(
                    startTime, endTime, organizationId, slot.SalonId, slot.Id))
                {
                    throw new ConflictException("Указанный интервал уже занят другим слотом.");
                }
            }
            else if (await _scheduleRepository.HasOverlapWithOccupiedSlotsAsync(
                startTime, endTime, organizationId, slot.SalonId, slot.Id, slot.Appointment.Id))
            {
                throw new ConflictException("Указанный интервал уже занят другой записью.");
            }
        }

        if (request.AppointmentData is not null && slot.Appointment is null)
        {
            throw new ValidationException("Нельзя обновить запись у свободного слота.");
        }

        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        if (request.StartTime.HasValue || request.EndTime.HasValue)
        {
            slot.StartTime = startTime;
            slot.EndTime = endTime;
            slot.UpdatedAt = DateTime.UtcNow;
            if (slot.Appointment is not null)
            {
                slot.Appointment.StartTime = startTime;
                slot.Appointment.EndTime = endTime;
                await _scheduleRepository.DeleteFreeSlotsByTimeAsync(
                    startTime, endTime, organizationId, slot.SalonId, slot.Id);
            }
        }

        if (request.AppointmentData is not null)
        {
            var appointmentData = request.AppointmentData;
            await _appointmentService.UpdateAsync(
                slot.Appointment!.Id,
                new UpdateAppointmentRequest(
                    appointmentData.ClientId,
                    appointmentData.Services,
                    null,
                    null,
                    appointmentData.Status,
                    appointmentData.Notes,
                    appointmentData.SalonId),
                cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private Guid GetOrganizationId()
    {
        return _organizationContext.GetCurrentOrganizationId()
            ?? throw new ValidationException("Не удалось определить текущую организацию.");
    }
}