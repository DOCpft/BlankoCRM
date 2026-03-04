using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BlankoCRM.Core.Entities;
using BlankoCRM.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace BlankoCRM.Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _dbContext;
    private readonly IPasswordHasher<Tenant> _passwordHasher;
    private readonly IConfiguration _configuration;

    public AuthController(
        AppDbContext dbContext,
        IPasswordHasher<Tenant> passwordHasher,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Email и пароль обязательны");
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var exists = await _dbContext.Tenants.AnyAsync(t => t.Email == normalizedEmail);
        if (exists)
        {
            return Conflict("Пользователь с таким email уже существует");
        }

        var tenant = new Tenant
        {
            Id = Guid.NewGuid(),
            Name = request.Name?.Trim() ?? normalizedEmail,
            Email = normalizedEmail,
            CreatedAt = DateTime.UtcNow
        };

        tenant.PasswordHash = _passwordHasher.HashPassword(tenant, request.Password);

        _dbContext.Tenants.Add(tenant);
        await _dbContext.SaveChangesAsync();

        return Ok();
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest("Email и пароль обязательны");
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var tenant = await _dbContext.Tenants.FirstOrDefaultAsync(t => t.Email == normalizedEmail);
        if (tenant is null)
        {
            return Unauthorized();
        }

        var verifyResult = _passwordHasher.VerifyHashedPassword(tenant, tenant.PasswordHash, request.Password);
        if (verifyResult == PasswordVerificationResult.Failed)
        {
            return Unauthorized();
        }

        var token = GenerateJwtToken(tenant);

        var response = new AuthResponse(token, tenant.Name, tenant.Email);

        return Ok(response);
    }

    private string GenerateJwtToken(Tenant tenant)
    {
        var jwtKey = _configuration["Jwt:Key"] ?? "dev-secret-key-change-in-production";
        var jwtIssuer = _configuration["Jwt:Issuer"] ?? "BlankoCRM";
        var jwtAudience = _configuration["Jwt:Audience"] ?? "BlankoCRMClient";

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, tenant.Id.ToString()),
            new Claim("tenantId", tenant.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, tenant.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
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

public record RegisterRequest(string Name, string Email, string Password);
public record LoginRequest(string Email, string Password);
public record AuthResponse(string Token, string Name, string Email);

