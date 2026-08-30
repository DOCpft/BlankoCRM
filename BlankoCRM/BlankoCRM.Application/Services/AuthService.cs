using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BlankoCRM.Core.Entities;
using BlankoCRM.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using BlankoCRM.Application.Abstract;

namespace BlankoCRM.Application.Services;

/// <summary>
/// Реализация сервиса аутентификации.
/// </summary>
public class AuthService : IAuthService
{
    private readonly AppDbContext _dbContext;
    private readonly IPasswordHasher<Tenant> _passwordHasher;
    private readonly IConfiguration _configuration;

    public AuthService(
        AppDbContext dbContext,
        IPasswordHasher<Tenant> passwordHasher,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }

    public async Task RegisterAsync(string? name, string email, string password)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        var exists = await _dbContext.Tenants.AnyAsync(t => t.Email == normalizedEmail);
        if (exists)
        {
            throw new InvalidOperationException("Пользователь с таким email уже существует");
        }

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = name?.Trim() ?? normalizedEmail,
            Email = normalizedEmail,
            CreatedAt = DateTime.UtcNow
        };

        tenant.PasswordHash = _passwordHasher.HashPassword(tenant, password);

        _dbContext.Tenants.Add(tenant);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<(string Token, string Name, string Email)> LoginAsync(string email, string password)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        var tenant = await _dbContext.Tenants.FirstOrDefaultAsync(t => t.Email == normalizedEmail)
            ?? throw new UnauthorizedAccessException("Неверный email или пароль");

        var verifyResult = _passwordHasher.VerifyHashedPassword(tenant, tenant.PasswordHash, password);
        if (verifyResult == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedAccessException("Неверный email или пароль");
        }

        var token = GenerateJwtToken(tenant);

        return (token, tenant.Name, tenant.Email);
    }

    private string GenerateJwtToken(Tenant tenant)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key is not configured.");
        var jwtIssuer = _configuration["Jwt:Issuer"] ?? "BlankoCRM";
        var jwtAudience = _configuration["Jwt:Audience"] ?? "BlankoCRMClient";

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, tenant.Id.ToString()),
            new("tenantId", tenant.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, tenant.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

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
