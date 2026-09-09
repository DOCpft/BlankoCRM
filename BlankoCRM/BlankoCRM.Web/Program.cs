using BlankoCRM.Application;
using BlankoCRM.Core.Entities;
using BlankoCRM.Core.Interfaces;
using BlankoCRM.Infrastructure;
using BlankoCRM.Infrastructure.Data;
using BlankoCRM.Web.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using FluentValidation.AspNetCore;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
           .LogTo(Console.WriteLine, LogLevel.Information)
           .EnableSensitiveDataLogging());

builder.Services.AddScoped<IOrganizationContext, OrganizationContext>();
builder.Services.AddInfrastructureRepositories();

builder.Services
    .AddIdentityCore<AppUser>()
    .AddRoles<AppRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

// Р РµРіРёСЃС‚СЂРёСЂСѓРµРј СЃРµСЂРІРёСЃС‹ СЃР»РѕСЏ Application
builder.Services.AddApplicationServices();

// JWT configuration
var jwtKey = builder.Configuration["Jwt:Key"] ?? "rQ5S8mK3vF6tY1wB7nH9xP2aC4dG0JkL6oV5uI3eX7=";//?? throw new InvalidOperationException("JWT Key is not configured. Set Jwt:Key in appsettings.json or user secrets.");
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
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BlankoCRM API", Version = "v1" });

    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Р’РІРµРґРёС‚Рµ С‚РѕРєРµРЅ РІ С„РѕСЂРјР°С‚Рµ: Bearer {token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    };

    c.AddSecurityDefinition("Bearer", jwtSecurityScheme);

    // Р”РѕР±Р°РІР»СЏРµРј SecurityRequirement, С‡С‚РѕР±С‹ Swagger UI РїСЂРёРјРµРЅСЏР» С‚РѕРєРµРЅ РєРѕ РІСЃРµРј СЌРЅРґРїРѕРёРЅС‚Р°Рј
    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

// РџСЂРёРјРµРЅСЏРµРј РјРёРіСЂР°С†РёРё (РґР»СЏ СЂР°Р·СЂР°Р±РѕС‚РєРё)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.UseHttpsRedirection();

app.UseSwagger();
app.UseSwaggerUI();

app.UseStaticFiles();

// Р“Р»РѕР±Р°Р»СЊРЅС‹Р№ middleware РґР»СЏ РѕР±СЂР°Р±РѕС‚РєРё РёСЃРєР»СЋС‡РµРЅРёР№ (РґРѕР»Р¶РµРЅ Р±С‹С‚СЊ РґРѕ РѕСЃС‚Р°Р»СЊРЅС‹С… middleware)
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// Middleware: СѓСЃС‚Р°РЅР°РІР»РёРІР°РµС‚ OrganizationId РёР· JWT РґР»СЏ Р°РІС‚РѕСЂРёР·РѕРІР°РЅРЅС‹С… Р·Р°РїСЂРѕСЃРѕРІ,
// Рё СЃР±СЂР°СЃС‹РІР°РµС‚ (РѕС‡РёС‰Р°РµС‚) РґР»СЏ РЅРµР°РІС‚РѕСЂРёР·РѕРІР°РЅРЅС‹С….
// Р­С‚Рѕ РєСЂРёС‚РёС‡РµСЃРєРё РІР°Р¶РЅРѕ, С‡С‚РѕР±С‹ РіР»РѕР±Р°Р»СЊРЅС‹Рµ Query Filters РЅРµ РїСЂРёРјРµРЅСЏР»РёСЃСЊ
// Рє РЅРµР°РІС‚РѕСЂРёР·РѕРІР°РЅРЅС‹Рј Р·Р°РїСЂРѕСЃР°Рј (СЂРµРіРёСЃС‚СЂР°С†РёСЏ, Р»РѕРіРёРЅ).
app.Use(async (context, next) =>
{
    // РЎР±СЂР°СЃС‹РІР°РµРј OrganizationId РІ РЅР°С‡Р°Р»Рµ РєР°Р¶РґРѕРіРѕ Р·Р°РїСЂРѕСЃР°
    var organizationContext = context.RequestServices.GetRequiredService<IOrganizationContext>();
    organizationContext.Clear();

    if (context.User.Identity?.IsAuthenticated == true)
    {
        var organizationIdClaim = context.User.FindFirst("organizationId")?.Value;

        if (Guid.TryParse(organizationIdClaim, out var organizationId))
        {
            organizationContext.SetCurrentOrganizationId(organizationId);
        }
    }

    try
    {
        await next();
    }
    finally
    {
        organizationContext.Clear();
    }
});

app.MapControllers();

app.Run();

