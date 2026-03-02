using BlankoCRM.Core;
using BlankoCRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add device-specific services used by the BlankoCRM.Shared project

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

// middleware компонент для определения текущего пользователя базы
app.Use(async (context, next) =>
{
    // Здесь получи TenantId из JWT-токена, заголовка или поддомена
    // Пока для разработки можно использовать фиксированный ID
    var tenantId = Guid.Parse("11111111-1111-1111-1111-111111111111"); // заменим позже
    TenantContext.CurrentTenantId = tenantId;
    await next();
});


app.Run();
