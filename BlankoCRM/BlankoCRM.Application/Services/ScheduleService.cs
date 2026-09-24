using System.Data;
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
        ValidateTimeRange(request);

        var ownsTransaction = !_unitOfWork.HasActiveTransaction;
        await using var transaction = ownsTransaction
            ? await _unitOfWork.BeginTransactionAsync(cancellationToken)
            : null;

        await EnsureNoOverlapAsync(request, organizationId, cancellationToken);
        var slot = CreateSlot(request, organizationId);
        await _scheduleRepository.AddAsync(slot, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (ownsTransaction)
        {
            await transaction!.CommitAsync(cancellationToken);
        }

        return slot;
    }

    public async Task<Schedule> AddOccupiedSlotAsync(CreateOccupiedSlotRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _appointmentService.CreateWithScheduleAsync(
            request.AppointmentData.ClientId,
            request.AppointmentData.Services,
            request.StartTime,
            request.AppointmentData.Notes,
            request.AppointmentData.SalonId,
            request.EndTime,
            cancellationToken);

        return result.Schedule;
    }

    public async Task<List<Schedule>> AddSlotsAsync(List<CreateFreeSlotRequest> addedSlots, CancellationToken cancellationToken = default)
    {
        if (addedSlots.Count == 0)
        {
            return [];
        }

        var organizationId = GetOrganizationId();
        if (_unitOfWork.HasActiveTransaction)
        {
            throw new InvalidOperationException(
                "Batch-добавление слотов не поддерживается внутри внешней транзакции.");
        }

        var slots = addedSlots
            .Select(request =>
            {
                ValidateTimeRange(request);
                return CreateSlot(request, organizationId);
            })
            .ToList();

        EnsureNoBatchOverlap(slots);

        const int maxRetries = 3;
        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                await using var transaction = await _unitOfWork.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

                foreach (var slot in slots)
                {
                    if (await _scheduleRepository.HasOverlapAsync(
                        slot.StartTime,
                        slot.EndTime,
                        organizationId,
                        slot.SalonId,
                        cancellationToken: cancellationToken))
                    {
                        _logger.LogWarning(
                            "Free slot conflict: interval {Start}-{End}, organization {OrganizationId}",
                            slot.StartTime,
                            slot.EndTime,
                            organizationId);
                        throw new ConflictException("Указанный интервал уже занят другим слотом.");
                    }
                }

                await _scheduleRepository.AddRangeAsync(slots, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return slots;
            }
            catch (Exception exception) when (_unitOfWork.IsSerializationFailure(exception))
            {
                _unitOfWork.ClearTracking();
                if (attempt == maxRetries)
                {
                    throw new ConflictException(
                        "Не удалось создать слоты из-за высокой конкуренции. Попробуйте ещё раз через пару секунд.");
                }

                await Task.Delay(Random.Shared.Next(10, 50), cancellationToken);
            }
        }

        throw new InvalidOperationException("Batch-добавление слотов завершилось без результата.");
    }

    private async Task EnsureNoOverlapAsync(
        CreateFreeSlotRequest request,
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        if (await _scheduleRepository.HasOverlapAsync(
            request.StartTime,
            request.EndTime,
            organizationId,
            request.SalonId,
            cancellationToken: cancellationToken))
        {
            _logger.LogWarning(
                "Free slot conflict: interval {Start}-{End}, organization {OrganizationId}",
                request.StartTime,
                request.EndTime,
                organizationId);
            throw new ConflictException("Указанный интервал уже занят другим слотом.");
        }
    }

    private static Schedule CreateSlot(CreateFreeSlotRequest request, Guid organizationId)
    {
        return new Schedule
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            SalonId = request.SalonId,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            CreatedAt = DateTime.UtcNow
        };
    }

    private static void ValidateTimeRange(CreateFreeSlotRequest request)
    {
        if (request.EndTime <= request.StartTime)
        {
            throw new ValidationException("Конечное время должно быть позже начального времени.");
        }
    }

    private static void EnsureNoBatchOverlap(IReadOnlyList<Schedule> slots)
    {
        for (var i = 0; i < slots.Count; i++)
        {
            for (var j = i + 1; j < slots.Count; j++)
            {
                var first = slots[i];
                var second = slots[j];
                if (first.SalonId == second.SalonId
                    && first.StartTime < second.EndTime
                    && first.EndTime > second.StartTime)
                {
                    throw new ConflictException("Слоты внутри batch-запроса пересекаются.");
                }
            }
        }
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
        var ownsTransaction = !_unitOfWork.HasActiveTransaction;
        await using var transaction = ownsTransaction
            ? await _unitOfWork.BeginTransactionAsync(cancellationToken)
            : null;

        if (slot.Appointment is not null)
        {
            slot.Appointment.Status = AppointmentStatus.Archived;
            slot.Appointment.UpdatedAt = DateTime.UtcNow;
            slot.AppointmentId = null;
            slot.Appointment = null;
        }

        _unitOfWork.Remove(slot);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        if (ownsTransaction)
        {
            await transaction!.CommitAsync(cancellationToken);
        }
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

        var ownsTransaction = !_unitOfWork.HasActiveTransaction;
        await using var transaction = ownsTransaction
            ? await _unitOfWork.BeginTransactionAsync(cancellationToken)
            : null;
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
        if (ownsTransaction)
        {
            await transaction!.CommitAsync(cancellationToken);
        }
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
                    startTime, endTime, organizationId, slot.SalonId, slot.Id, cancellationToken))
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

        var ownsTransaction = !_unitOfWork.HasActiveTransaction;
        await using var transaction = ownsTransaction
            ? await _unitOfWork.BeginTransactionAsync(cancellationToken)
            : null;
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
        if (ownsTransaction)
        {
            await transaction!.CommitAsync(cancellationToken);
        }
    }

    private Guid GetOrganizationId()
    {
        return _organizationContext.GetCurrentOrganizationId()
            ?? throw new ValidationException("Не удалось определить текущую организацию.");
    }
}