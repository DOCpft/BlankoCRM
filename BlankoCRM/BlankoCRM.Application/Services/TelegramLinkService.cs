using BlankoCRM.Application.Abstract;
using BlankoCRM.Application.DTOs;
using BlankoCRM.Core.Entities;
using BlankoCRM.Core.Exceptions;
using BlankoCRM.Core.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BlankoCRM.Application.Services;

/// <summary>
/// Управляет привязкой Telegram-аккаунта мастера.
/// </summary>
public sealed class TelegramLinkService : ITelegramLinkService
{
    private const int LinkCodeLifetimeMinutes = 10;

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<AppUser> _userManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITelegramBotClient _telegramBotClient;
    private readonly IConfiguration _configuration;

    public TelegramLinkService(
        IHttpContextAccessor httpContextAccessor,
        UserManager<AppUser> userManager,
        IUnitOfWork unitOfWork,
        ITelegramBotClient telegramBotClient,
        IConfiguration configuration)
    {
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
        _unitOfWork = unitOfWork;
        _telegramBotClient = telegramBotClient;
        _configuration = configuration;
    }

    public async Task<TelegramLinkResponse> GenerateLinkAsync(CancellationToken ct = default)
    {
        var user = await GetCurrentUserAsync(ct);
        var code = Guid.NewGuid().ToString("N")[..8];
        var botUsername = _configuration["Telegram:BotUsername"] ?? "blanko_crm_bot";

        var ownsTransaction = !_unitOfWork.HasActiveTransaction;
        await using var transaction = ownsTransaction ? await _unitOfWork.BeginTransactionAsync(ct) : null;
        try
        {
            user.TelegramLinkCode = code;
            user.TelegramLinkCodeExpiresAt = DateTime.UtcNow.AddMinutes(LinkCodeLifetimeMinutes);
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

        return new TelegramLinkResponse($"https://t.me/{botUsername}?start={code}", botUsername);
    }

    public async Task UnlinkAsync(CancellationToken ct = default)
    {
        var user = await GetCurrentUserAsync(ct);

        var ownsTransaction = !_unitOfWork.HasActiveTransaction;
        await using var transaction = ownsTransaction ? await _unitOfWork.BeginTransactionAsync(ct) : null;
        try
        {
            user.TelegramChatId = null;
            user.TelegramLinkCode = null;
            user.TelegramLinkCodeExpiresAt = null;
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

    public async Task<TelegramLinkStatus> GetStatusAsync(CancellationToken ct = default)
    {
        var user = await GetCurrentUserAsync(ct);
        return new TelegramLinkStatus(user.TelegramChatId.HasValue, user.TelegramNotificationsEnabled);
    }

    public async Task HandleStartCommandAsync(string code, long chatId, CancellationToken ct = default)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(
            u => u.TelegramLinkCode == code && u.TelegramLinkCodeExpiresAt > DateTime.UtcNow,
            ct);

        if (user is null)
        {
            await _telegramBotClient.SendMessageAsync(
                chatId,
                "Код недействителен. Сгенерируйте новый в личном кабинете.",
                ct);
            return;
        }

        var ownsTransaction = !_unitOfWork.HasActiveTransaction;
        await using var transaction = ownsTransaction ? await _unitOfWork.BeginTransactionAsync(ct) : null;
        try
        {
            user.TelegramChatId = chatId;
            user.TelegramLinkCode = null;
            user.TelegramLinkCodeExpiresAt = null;
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

        await _telegramBotClient.SendMessageAsync(
            chatId,
            "Теперь уведомления о записях будут приходить Вам сюда! Хорошей работы)",
            ct);
    }

    private async Task<AppUser> GetCurrentUserAsync(CancellationToken ct)
    {
        var principal = _httpContextAccessor.HttpContext?.User
            ?? throw new UnauthorizedException("Пользователь не авторизован.");

        return await _userManager.GetUserAsync(principal)
            ?? throw new UnauthorizedException("Пользователь не найден.");
    }
}
