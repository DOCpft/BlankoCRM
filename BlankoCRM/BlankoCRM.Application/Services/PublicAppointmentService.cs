using BlankoCRM.Application.Abstract;
using BlankoCRM.Application.DTOs;
using BlankoCRM.Core.Entities;
using BlankoCRM.Core.Exceptions;
using BlankoCRM.Core.Interfaces;
using Microsoft.Extensions.Logging;
using AppointmentServiceEntity = BlankoCRM.Core.Entities.AppointmentService;

namespace BlankoCRM.Application.Services;

/// <summary>
/// Создаёт клиентскую заявку в статусе Pending без создания записи расписания.
/// </summary>
public sealed class PublicAppointmentService : IPublicAppointmentService
{
    private const int PendingLifetimeHours = 24;
    private const int MaxUniqueViolationAttempts = 3;
    private const int MaxActivePendingPerClient = 5;

    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrganizationContext _organizationContext;
    private readonly IVkMiniAppAuthenticator _vkMiniAppAuthenticator;
    private readonly IPublicVkRateLimiter _vkRateLimiter;
    private readonly IPublicClientResolver _clientResolver;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IRepository<Core.Entities.Service> _serviceRepository;
    private readonly ITelegramNotificationService _telegramNotificationService;
    private readonly ILogger<PublicAppointmentService> _logger;

    public PublicAppointmentService(
        IUnitOfWork unitOfWork,
        IOrganizationContext organizationContext,
        IVkMiniAppAuthenticator vkMiniAppAuthenticator,
        IPublicVkRateLimiter vkRateLimiter,
        IPublicClientResolver clientResolver,
        IAppointmentRepository appointmentRepository,
        IClientRepository clientRepository,
        IRepository<Core.Entities.Service> serviceRepository,
        ITelegramNotificationService telegramNotificationService,
        ILogger<PublicAppointmentService> logger)
    {
        _unitOfWork = unitOfWork;
        _organizationContext = organizationContext;
        _vkMiniAppAuthenticator = vkMiniAppAuthenticator;
        _vkRateLimiter = vkRateLimiter;
        _clientResolver = clientResolver;
        _appointmentRepository = appointmentRepository;
        _clientRepository = clientRepository;
        _serviceRepository = serviceRepository;
        _telegramNotificationService = telegramNotificationService;
        _logger = logger;
    }

    public async Task<Appointment> CreatePendingAsync(
        CreatePublicAppointmentRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var organizationId = _organizationContext.GetCurrentOrganizationId()
            ?? throw new UnauthorizedAccessException("Organization context is not initialized.");
        if (request.VkParams is null)
        {
            throw new ValidationException("Параметры VK обязательны.");
        }

        var verifiedVkUserId = _vkMiniAppAuthenticator.Authenticate(request.VkParams);
        if (!_vkRateLimiter.TryAcquire(verifiedVkUserId, PublicAppointmentOperation.Create))
        {
            throw new TooManyRequestsException("Превышен лимит создания заявок. Повторите попытку позже.");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ValidationException("Имя клиента обязательно.");
        }

        if (request.Services is null || request.Services.Count == 0)
        {
            throw new ValidationException("Необходимо указать хотя бы одну услугу.");
        }

        if (request.StartTime == default)
        {
            throw new ValidationException("Время начала обязательно.");
        }

        var startTime = request.StartTime.UtcDateTime;
        if (startTime <= DateTime.UtcNow)
        {
            throw new ValidationException("Нельзя создать заявку на прошедшее время.");
        }

        var serviceIds = request.Services.Select(input => input.ServiceId).Distinct().ToList();
        var services = (await _serviceRepository.FindAsync(
                service => serviceIds.Contains(service.Id) && service.OrganizationId == organizationId,
                cancellationToken))
            .ToDictionary(service => service.Id);

        var missingServiceIds = serviceIds.Except(services.Keys).ToList();
        if (missingServiceIds.Count > 0)
        {
            throw new NotFoundException(
                $"Услуги не найдены в текущей организации: {string.Join(", ", missingServiceIds)}.");
        }

        var appointmentServices = new List<AppointmentServiceEntity>(request.Services.Count);
        foreach (var input in request.Services)
        {
            var service = services[input.ServiceId];
            var duration = service.DurationMinutes;
            var quantity = input.Quantity;
            var price = service.Price;

            if (duration <= 0 || quantity <= 0 || price < 0)
            {
                throw new ValidationException("Некорректные длительность, количество или стоимость услуги.");
            }

            appointmentServices.Add(new AppointmentServiceEntity
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                ServiceId = service.Id,
                ServiceName = service.Name,
                DurationMinutes = duration,
                Price = price,
                Quantity = quantity
            });
        }

        long durationMinutes;
        try
        {
            durationMinutes = appointmentServices.Sum(item => (long)item.DurationMinutes * item.Quantity);
        }
        catch (OverflowException)
        {
            throw new ValidationException("Суммарная длительность услуг слишком велика.");
        }

        if (durationMinutes <= 0
            || durationMinutes > int.MaxValue
            || durationMinutes > (DateTime.MaxValue - startTime).TotalMinutes)
        {
            throw new ValidationException("Суммарная длительность услуг выходит за допустимый диапазон времени.");
        }

        var endTime = startTime.AddMinutes(durationMinutes);
        if (endTime <= startTime)
        {
            throw new ValidationException("Время окончания должно быть позже времени начала.");
        }

        for (var attempt = 1; attempt <= MaxUniqueViolationAttempts; attempt++)
        {
            try
            {
                var appointment = await CreatePendingAttemptAsync(
                    request,
                    organizationId,
                    verifiedVkUserId,
                    appointmentServices,
                    startTime,
                    endTime,
                    cancellationToken);

                await NotifyMasterAboutPendingAsync(appointment, cancellationToken);
                return appointment;
            }
            catch (Exception exception) when (
                _unitOfWork.IsUniqueViolation(exception) && !_unitOfWork.HasActiveTransaction)
            {
                _unitOfWork.ClearTracking();
                if (attempt == MaxUniqueViolationAttempts)
                {
                    throw new ConflictException(
                        "Не удалось обработать заявку из-за параллельного изменения данных клиента. Повторите попытку.");
                }

                _logger.LogInformation(
                    exception,
                    "Retrying public appointment creation after a unique constraint conflict for organization {OrganizationId}",
                    organizationId);
            }
        }

        throw new InvalidOperationException("Создание публичной заявки завершилось без результата.");
    }

    private async Task NotifyMasterAboutPendingAsync(Appointment appointment, CancellationToken cancellationToken)
    {
        try
        {
            await _telegramNotificationService.NotifyMasterNewPendingAsync(appointment, cancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Не удалось отправить Telegram-уведомление о новой заявке {AppointmentId}.",
                appointment.Id);
        }
    }

    public async Task WithdrawAsync(
        Guid appointmentId,
        IReadOnlyDictionary<string, string> vkParams,
        CancellationToken cancellationToken = default)
    {
        if (vkParams is null)
        {
            throw new ValidationException("Параметры VK обязательны.");
        }

        var organizationId = _organizationContext.GetCurrentOrganizationId()
            ?? throw new UnauthorizedAccessException("Organization context is not initialized.");
        var verifiedVkUserId = _vkMiniAppAuthenticator.Authenticate(vkParams);
        if (!_vkRateLimiter.TryAcquire(verifiedVkUserId, PublicAppointmentOperation.Withdraw))
        {
            throw new TooManyRequestsException("Превышен лимит отзыва заявок. Повторите попытку позже.");
        }
        var ownsTransaction = !_unitOfWork.HasActiveTransaction;
        await using var transaction = ownsTransaction
            ? await _unitOfWork.BeginTransactionAsync(cancellationToken)
            : null;

        try
        {
            await _appointmentRepository.LockForUpdateAsync(organizationId, appointmentId, cancellationToken);
            var client = await _clientRepository.FindByVkUserIdAsync(
                organizationId, verifiedVkUserId, cancellationToken);
            var appointment = await _appointmentRepository.GetByIdWithDetailsAsync(
                appointmentId, cancellationToken);

            if (client is null
                || appointment is null
                || appointment.OrganizationId != organizationId
                || appointment.ClientId != client.Id)
            {
                throw new NotFoundException("Заявка не найдена.");
            }

            if (appointment.Status != AppointmentStatus.Pending)
            {
                throw new ConflictException("Отозвать можно только ожидающую заявку.");
            }

            if (!appointment.ExpiresAt.HasValue || appointment.ExpiresAt.Value <= DateTime.UtcNow)
            {
                throw new ConflictException("Срок действия заявки истёк.");
            }

            appointment.Status = AppointmentStatus.Withdrawn;
            appointment.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            if (ownsTransaction)
            {
                await transaction!.CommitAsync(cancellationToken);
            }
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

    private async Task<Appointment> CreatePendingAttemptAsync(
        CreatePublicAppointmentRequest request,
        Guid organizationId,
        long verifiedVkUserId,
        List<AppointmentServiceEntity> appointmentServices,
        DateTime startTime,
        DateTime endTime,
        CancellationToken cancellationToken)
    {
        var ownsTransaction = !_unitOfWork.HasActiveTransaction;
        await using var transaction = ownsTransaction
            ? await _unitOfWork.BeginTransactionAsync(cancellationToken)
            : null;

        try
        {
            var client = await _clientResolver.ResolveAsync(
                organizationId,
                verifiedVkUserId,
                request.Name,
                request.Phone,
                cancellationToken);

            await _clientRepository.LockForPendingLimitAsync(client.Id, cancellationToken);
            var activePendingCount = await _appointmentRepository.CountActivePendingAsync(
                organizationId,
                client.Id,
                DateTime.UtcNow,
                cancellationToken);
            if (activePendingCount >= MaxActivePendingPerClient)
            {
                throw new TooManyRequestsException(
                    $"У клиента уже есть максимальное количество активных заявок ({MaxActivePendingPerClient}).");
            }

            var now = DateTime.UtcNow;
            var appointment = new Appointment
            {
                Id = Guid.NewGuid(),
                OrganizationId = organizationId,
                SalonId = null,
                ClientId = client.Id,
                Client = client,
                StartTime = startTime,
                EndTime = endTime,
                Status = AppointmentStatus.Pending,
                ExpiresAt = now.AddHours(PendingLifetimeHours),
                Notes = request.Notes,
                ReminderSent = false,
                CreatedAt = now,
                AppointmentServices = appointmentServices.Select(item => new AppointmentServiceEntity
                {
                    Id = item.Id,
                    OrganizationId = item.OrganizationId,
                    ServiceId = item.ServiceId,
                    ServiceName = item.ServiceName,
                    DurationMinutes = item.DurationMinutes,
                    Price = item.Price,
                    Quantity = item.Quantity
                }).ToList()
            };

            foreach (var appointmentService in appointment.AppointmentServices)
            {
                appointmentService.AppointmentId = appointment.Id;
                appointmentService.Appointment = appointment;
            }

            _unitOfWork.Add(appointment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            if (ownsTransaction)
            {
                await transaction!.CommitAsync(cancellationToken);
            }

            return appointment;
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
