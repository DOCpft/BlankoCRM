using BlankoCRM.Application;
using BlankoCRM.Core.Entities;
using BlankoCRM.Infrastructure;
using BlankoCRM.Infrastructure.Data;
using BlankoCRM.Web.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
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

builder.Services.AddScoped<IPasswordHasher<Tenant>, PasswordHasher<Tenant>>();

// Регистрируем сервисы слоя Application
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
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BlankoCRM API", Version = "v1" });

    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Введите токен в формате: Bearer {token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    };

    c.AddSecurityDefinition("Bearer", jwtSecurityScheme);

    // Добавляем SecurityRequirement, чтобы Swagger UI применял токен ко всем эндпоинтам
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

// Применяем миграции (для разработки)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.UseHttpsRedirection();

app.UseSwagger();
app.UseSwaggerUI();

app.UseStaticFiles();

// Глобальный middleware для обработки исключений (должен быть до остальных middleware)
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// Middleware: устанавливает TenantId из JWT для авторизованных запросов,
// и сбрасывает (очищает) для неавторизованных.
// Это критически важно, чтобы глобальные Query Filters не применялись
// к неавторизованным запросам (регистрация, логин).
app.Use(async (context, next) =>
{
    // Сбрасываем TenantId в начале каждого запроса
    TenantContext.Clear();

    if (context.User.Identity?.IsAuthenticated == true)
    {
        var tenantIdClaim = context.User.FindFirst("tenantId")?.Value
                            ?? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            TenantContext.CurrentTenantId = tenantId;
        }
    }

    await next();
});

app.MapControllers();

app.Run();
