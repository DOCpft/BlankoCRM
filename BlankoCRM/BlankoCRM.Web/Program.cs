using BlankoCRM.Application;
using BlankoCRM.Application.Abstract;
using BlankoCRM.Core.Entities;
using BlankoCRM.Core.Interfaces;
using BlankoCRM.Infrastructure;
using BlankoCRM.Infrastructure.Data;
using BlankoCRM.Web.Extensions;
using BlankoCRM.Web.BackgroundServices;
using BlankoCRM.Web.Middleware;
using BlankoCRM.Web.Swagger;
using BlankoCRM.Web.RateLimiting;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Prometheus;
using Prometheus.SystemMetrics;
using Serilog.Context;
using Swashbuckle.AspNetCore.Annotations;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseBlankoSerilog();

builder.Services.AddSystemMetrics();
// Add services to the container.
//builder.Services.AddDbContext<AppDbContext>(options =>
//    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IOrganizationContext, OrganizationContext>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddInfrastructureRepositories();

builder.Services
    .AddIdentityCore<AppUser>()
    .AddRoles<AppRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

// Регистрируем сервисы слоя Application
builder.Services.AddApplicationServices();
builder.Services.AddSingleton<IPublicVkRateLimiter, PublicVkRateLimiter>();
builder.Services.AddHostedService<PendingAppointmentExpirationWorker>();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("public-appointment-create", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
    options.AddPolicy("public-appointment-withdraw", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 20,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});

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
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
    options.ForwardLimit = 1;
});
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SchemaFilter<RequiredPropertiesSchemaFilter>();

    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BlankoCRM API",
        Version = "v1",
        Description = "API для управления записями клиентов, услугами и расписанием.",
        Contact = new OpenApiContact
        {
            Name = "Support",
            Email = "support@blanko.ru"
        },
        License = new OpenApiLicense
        {
            Name = "MIT",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });

    // XML комментарии
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    // XML комментарии из слоя Application (DTO)
    var applicationXmlFile = "BlankoCRM.Application.xml";
    var applicationXmlPath = Path.Combine(AppContext.BaseDirectory, applicationXmlFile);
    if (File.Exists(applicationXmlPath))
    {
        c.IncludeXmlComments(applicationXmlPath);
    }

    // Аннотации для операций и примеров
    c.EnableAnnotations();

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

app.UseForwardedHeaders();

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

app.UseMiddleware<CorrelationIdMiddleware>();

if (!app.Environment.IsDevelopment()) // в проде nginx пришлет X-Forwarded-Proto: https, и этот редирект просто не сработает, что хорошо. А приложение внутри docker-сети слушает http. В dev среде же редирект в приложении сработает (самоподписанный TLS не катит для Prometheus). Трафик в интернет выходить не будет, поэтому использовать домен + Let's Encrypt не обязательно, лишняя заморочка для dev (хоть и в prod это будет, но не к чему заморачиваться). Проще говоря, не редиректим /metrics на https - оверхед.
{
    app.UseHttpsRedirection();
}

// Swagger размещён до request logging, чтобы не засорять прикладные логи.
app.UseSwagger();
app.UseSwaggerUI();

app.UseStaticFiles();

app.UseBlankoRequestLogging();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();


// Middleware устанавливает OrganizationId из JWT для авторизованных запросов
// и сбрасывает его для неавторизованных.
// Это критически важно, чтобы глобальные Query Filters не применялись
// к неавторизованным запросам (регистрация, логин).
app.Use(async (context, next) =>
{
    // Сбрасываем OrganizationId в начале каждого запроса
    var organizationContext = context.RequestServices.GetRequiredService<IOrganizationContext>();
    organizationContext.Clear();

    Guid? currentOrganizationId = null;
    if (context.User.Identity?.IsAuthenticated == true)
    {
        var organizationIdClaim = context.User.FindFirst("organizationId")?.Value;

        if (Guid.TryParse(organizationIdClaim, out var organizationId))
        {
            currentOrganizationId = organizationId;
            organizationContext.SetCurrentOrganizationId(organizationId);
        }
    }

    try
    {
        if (currentOrganizationId.HasValue)
        {
            using (LogContext.PushProperty("OrganizationId", currentOrganizationId.Value))
            {
                await next();
            }
        }
        else
        {
            await next();
        }
    }
    finally
    {
        organizationContext.Clear();
    }
});

// Для публичных endpoint'ов (/api/public/{slug}/...) организация разрешается
// по slug из маршрута после установки/сброса JWT-контекста организации.
app.UseMiddleware<PublicOrganizationMiddleware>();

app.UseHttpMetrics();   // Собирает метрики по HTTP-запросам
app.MapMetrics();       // Открывает эндпоинт /metrics
app.MapControllers();

app.Run();

