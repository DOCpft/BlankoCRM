using BlankoCRM.Application.Abstract;
using BlankoCRM.Core.Exceptions;
using BlankoCRM.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace BlankoCRM.Application.Services;

/// <summary>
/// Проверяет подпись, идентификатор пользователя и актуальность параметров VK Mini App.
/// </summary>
public sealed class VkMiniAppAuthenticator : IVkMiniAppAuthenticator
{
    private readonly IVkSignatureValidator _signatureValidator;
    private readonly TimeSpan _maxParameterAge;
    private readonly TimeSpan _futureClockSkew;

    public VkMiniAppAuthenticator(
        IVkSignatureValidator signatureValidator,
        IConfiguration configuration)
    {
        _signatureValidator = signatureValidator;
        _maxParameterAge = ReadPositiveSeconds(configuration, "VkApp:MaxParameterAgeSeconds", 300);
        _futureClockSkew = ReadPositiveSeconds(configuration, "VkApp:FutureClockSkewSeconds", 60);
    }

    public long Authenticate(IReadOnlyDictionary<string, string> vkParams)
    {
        if (!vkParams.TryGetValue("sign", out var sign) || string.IsNullOrWhiteSpace(sign))
        {
            throw new UnauthorizedException("Отсутствует подпись VK.");
        }

        if (!_signatureValidator.Validate(vkParams, sign))
        {
            throw new UnauthorizedException("Недействительная подпись VK.");
        }

        if (!vkParams.TryGetValue("vk_user_id", out var vkUserIdValue)
            || !long.TryParse(vkUserIdValue, NumberStyles.None, CultureInfo.InvariantCulture, out var vkUserId)
            || vkUserId <= 0)
        {
            throw new UnauthorizedException("Некорректный vk_user_id.");
        }

        if (!vkParams.TryGetValue("vk_ts", out var timestampValue)
            || !long.TryParse(timestampValue, NumberStyles.None, CultureInfo.InvariantCulture, out var timestamp))
        {
            throw new UnauthorizedException("Отсутствует или некорректен vk_ts.");
        }

        DateTimeOffset issuedAt;
        try
        {
            issuedAt = DateTimeOffset.FromUnixTimeSeconds(timestamp);
        }
        catch (ArgumentOutOfRangeException)
        {
            throw new UnauthorizedException("Некорректный vk_ts.");
        }

        var now = DateTimeOffset.UtcNow;
        if (issuedAt < now - _maxParameterAge || issuedAt > now + _futureClockSkew)
        {
            throw new UnauthorizedException("Срок действия параметров VK истёк.");
        }

        return vkUserId;
    }

    private static TimeSpan ReadPositiveSeconds(IConfiguration configuration, string key, int defaultValue)
    {
        return int.TryParse(configuration[key], NumberStyles.None, CultureInfo.InvariantCulture, out var seconds)
            && seconds > 0
                ? TimeSpan.FromSeconds(seconds)
                : TimeSpan.FromSeconds(defaultValue);
    }
}
