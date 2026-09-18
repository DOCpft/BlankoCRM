using BlankoCRM.Application.Abstract;
using BlankoCRM.Core.Entities;
using BlankoCRM.Core.Exceptions;
using BlankoCRM.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BlankoCRM.Application.Services;

/// <summary>
/// Сервис регистрации пользователей и выдачи JWT-токенов через ASP.NET Core Identity.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly IVkSignatureValidator _vkSignatureValidator;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUnitOfWork unitOfWork,
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        IConfiguration configuration,
        IVkSignatureValidator vkSignatureValidator,
        ILogger<AuthService> logger)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _vkSignatureValidator = vkSignatureValidator;
        _logger = logger;
    }

    public async Task RegisterAsync(string? name, string email, string password, string? organizationName)
    {
        var normalizedEmail = email.Trim();
        var organization = new Organization
        {
            Id = Guid.NewGuid(),
            Name = string.IsNullOrWhiteSpace(organizationName)
                ? (string.IsNullOrWhiteSpace(name) ? normalizedEmail : name.Trim())
                : organizationName.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await using var transaction = await _unitOfWork.BeginTransactionAsync();
        _unitOfWork.Add(organization);
        await _unitOfWork.SaveChangesAsync();

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = normalizedEmail,
            Email = normalizedEmail,
            Name = string.IsNullOrWhiteSpace(name) ? normalizedEmail : name.Trim(),
            OrganizationId = organization.Id
        };

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            await transaction.RollbackAsync();
            throw new ValidationException(string.Join("; ", result.Errors.Select(error => error.Description)));
        }

        await transaction.CommitAsync();
        _logger.LogInformation("User {UserId} registered with email {Email}", user.Id, normalizedEmail);
    }

    public async Task<(string Token, string Name, string Email)> LoginAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email.Trim());
        if (user is null)
        {
            _logger.LogWarning("Failed login attempt for email {Email}", email.Trim());
            throw new UnauthorizedException("Неверный email или пароль");
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
        if (!signInResult.Succeeded)
        {
            _logger.LogWarning("Failed login attempt for email {Email}", email.Trim());
            throw new UnauthorizedException("Неверный email или пароль");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = GenerateJwtToken(user, roles);
        _logger.LogInformation("User {UserId} logged in successfully", user.Id);
        return (token, user.Name, user.Email ?? string.Empty);
    }

    public async Task<(string Token, string Name, string Email)> AuthVkMiniAppAsync(
    IReadOnlyDictionary<string, string> vkParams,
    CancellationToken cancellationToken = default)
    {
        // 1. Извлекаем sign
        if (!vkParams.TryGetValue("sign", out var sign) || string.IsNullOrWhiteSpace(sign))
        {
            _logger.LogWarning("VK login rejected: missing signature");
            throw new UnauthorizedException("Отсутствует подпись VK.");
        }

        // 2. Проверяем подпись
        if (!_vkSignatureValidator.Validate(vkParams, sign))
        {
            _logger.LogWarning("VK login rejected: invalid signature");
            throw new UnauthorizedException("Недействительная подпись VK.");
        }

        // 3. Извлекаем vk_user_id (только после проверки подписи)
        if (!vkParams.TryGetValue("vk_user_id", out var vkUserIdStr)
            || !long.TryParse(vkUserIdStr, out var vkUserId))
        {
            throw new UnauthorizedException("Некорректный vk_user_id.");
        }

        // 4. Ищем существующего пользователя
        var existingUser = await _userManager.Users
            .FirstOrDefaultAsync(u => u.VkUserId == vkUserId, cancellationToken);

        if (existingUser is not null)
        {
            var existingRoles = await _userManager.GetRolesAsync(existingUser);
            var existingToken = GenerateJwtToken(existingUser, existingRoles);
            _logger.LogInformation("VK user {UserId} logged in successfully", existingUser.Id);
            return (existingToken, existingUser.Name, existingUser.Email ?? string.Empty);
        }

        // 5. Первый вход — создаём организацию и пользователя в одной транзакции
        var organization = new Organization
        {
            Id = Guid.NewGuid(),
            Name = $"VK {vkUserId}",
            CreatedAt = DateTime.UtcNow
        };

        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        _unitOfWork.Add(organization);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var newUser = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = $"vk_{vkUserId}",
            Email = null,
            Name = $"VK {vkUserId}",
            OrganizationId = organization.Id,
            VkUserId = vkUserId
        };

        var result = await _userManager.CreateAsync(newUser);
        if (!result.Succeeded)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw new ValidationException(
                string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        await transaction.CommitAsync(cancellationToken);

        var token = GenerateJwtToken(newUser, Array.Empty<string>());
        _logger.LogInformation("VK user {UserId} registered successfully", newUser.Id);
        return (token, newUser.Name, newUser.Email ?? string.Empty);
    }

    private string GenerateJwtToken(AppUser user, IEnumerable<string> roles)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured.");
        var jwtIssuer = _configuration["Jwt:Issuer"] ?? "BlankoCRM";
        var jwtAudience = _configuration["Jwt:Audience"] ?? "BlankoCRMClient";
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new("organizationId", user.OrganizationId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        if (user.SalonId.HasValue)
        {
            claims.Add(new Claim("salonId", user.SalonId.Value.ToString()));
        }

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
