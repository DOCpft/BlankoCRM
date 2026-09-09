using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BlankoCRM.Application.Abstract;
using BlankoCRM.Core.Entities;
using BlankoCRM.Core.Exceptions;
using BlankoCRM.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BlankoCRM.Application.Services;

/// <summary>
/// Сервис регистрации пользователей и выдачи JWT-токенов через ASP.NET Core Identity.
/// </summary>
public class AuthService : IAuthService
{
    private readonly AppDbContext _dbContext;
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly IConfiguration _configuration;

    public AuthService(
        AppDbContext dbContext,
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
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

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        _dbContext.Organizations.Add(organization);
        await _dbContext.SaveChangesAsync();

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
    }

    public async Task<(string Token, string Name, string Email)> LoginAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email.Trim());
        if (user is null)
        {
            throw new UnauthorizedException("Неверный email или пароль");
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
        if (!signInResult.Succeeded)
        {
            throw new UnauthorizedException("Неверный email или пароль");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var token = GenerateJwtToken(user, roles);
        return (token, user.Name, user.Email ?? string.Empty);
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
