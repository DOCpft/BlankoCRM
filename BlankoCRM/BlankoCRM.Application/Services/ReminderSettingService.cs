using BlankoCRM.Application.Abstract;
using BlankoCRM.Application.DTOs;
using BlankoCRM.Core.Entities;
using BlankoCRM.Core.Exceptions;
using BlankoCRM.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace BlankoCRM.Application.Services;

/// <summary>
/// Управляет настройками напоминаний текущего мастера.
/// </summary>
public sealed class ReminderSettingService : IReminderSettingService
{
    private static readonly int[] AllowedOffsets = [1, 3, 6, 12, 24];

    private readonly IMasterReminderSettingRepository _settingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<AppUser> _userManager;

    public ReminderSettingService(
        IMasterReminderSettingRepository settingRepository,
        IUnitOfWork unitOfWork,
        IHttpContextAccessor httpContextAccessor,
        UserManager<AppUser> userManager)
    {
        _settingRepository = settingRepository;
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
    }

    public async Task<List<ReminderSettingDto>> GetAllAsync(CancellationToken ct = default)
    {
        var user = await GetCurrentUserAsync(ct);
        var settings = await _settingRepository.GetByAppUserIdAsync(user.Id, ct);

        return settings
            .OrderBy(setting => setting.OffsetHours)
            .Select(setting => new ReminderSettingDto(setting.OffsetHours, setting.Enabled))
            .ToList();
    }

    public async Task UpdateAsync(UpdateReminderSettingsRequest request, CancellationToken ct = default)
    {
        Validate(request);

        var user = await GetCurrentUserAsync(ct);
        var existing = (await _settingRepository.GetByAppUserIdAsync(user.Id, ct))
            .ToDictionary(setting => setting.OffsetHours);

        var ownsTransaction = !_unitOfWork.HasActiveTransaction;
        await using var transaction = ownsTransaction ? await _unitOfWork.BeginTransactionAsync(ct) : null;
        try
        {
            foreach (var setting in request.Settings)
            {
                if (existing.TryGetValue(setting.OffsetHours, out var entity))
                {
                    entity.Enabled = setting.Enabled;
                }
            }

            await _unitOfWork.SaveChangesAsync(ct);
            if (ownsTransaction)
            {
                await transaction!.CommitAsync(ct);
            }
        }
        catch
        {
            if (ownsTransaction && _unitOfWork.HasActiveTransaction)
            {
                await transaction!.RollbackAsync(ct);
            }

            throw;
        }
    }

    private static void Validate(UpdateReminderSettingsRequest request)
    {
        if (request.Settings is null || request.Settings.Count == 0)
        {
            throw new ValidationException("Необходимо указать хотя бы одну настройку напоминаний.");
        }

        foreach (var setting in request.Settings)
        {
            if (!AllowedOffsets.Contains(setting.OffsetHours))
            {
                throw new ValidationException($"Недопустимый offset часов: {setting.OffsetHours}.");
            }
        }
    }

    private async Task<AppUser> GetCurrentUserAsync(CancellationToken ct)
    {
        var principal = _httpContextAccessor.HttpContext?.User
            ?? throw new UnauthorizedException("Пользователь не авторизован.");

        return await _userManager.GetUserAsync(principal)
            ?? throw new UnauthorizedException("Пользователь не найден.");
    }
}
