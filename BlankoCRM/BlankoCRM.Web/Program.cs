using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BlankoCRM.Core;
using BlankoCRM.Core.Entities;
using BlankoCRM.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IPasswordHasher<Tenant>, PasswordHasher<Tenant>>();

// JWT configuration (for development you can keep defaults, but better move to appsettings.json)
var jwtKey = builder.Configuration["Jwt:Key"] ?? "dev-secret-key-change-in-production";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "BlankoCRM";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "BlankoCRMClient";
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseSwagger();
app.UseSwaggerUI();

app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

// middleware: TenantId �� JWT
app.Use(async (context, next) =>
{
    var tenantIdClaim = context.User.FindFirst("tenantId")?.Value
                        ?? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    if (Guid.TryParse(tenantIdClaim, out var tenantId))
    {
        TenantContext.CurrentTenantId = tenantId;
    }

    await next();
});

app.MapControllers();

app.Run();
