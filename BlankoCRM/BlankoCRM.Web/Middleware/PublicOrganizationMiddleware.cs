using System.Net;
using System.Text.Json;
using BlankoCRM.Core.Interfaces;
using BlankoCRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BlankoCRM.Web.Middleware;

/// <summary>
/// Разрешает организацию по slug из маршрута публичных endpoint'ов
/// (/api/public/{organizationSlug}/...) и устанавливает её в IOrganizationContext.
/// Публичные запросы не используют JWT, поэтому организация определяется
/// только из slug маршрута — не из тела запроса и не из VK-параметров.
/// </summary>
public sealed class PublicOrganizationMiddleware
{
    private const string PublicApiPrefix = "/api/public/";

    private readonly RequestDelegate _next;
    private readonly ILogger<PublicOrganizationMiddleware> _logger;

    public PublicOrganizationMiddleware(RequestDelegate next, ILogger<PublicOrganizationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        AppDbContext dbContext,
        IOrganizationContext organizationContext)
    {
        if (!TryGetOrganizationSlug(context.Request.Path, out var slug))
        {
            await _next(context);
            return;
        }

        var organizationId = await dbContext.Organizations
            .AsNoTracking()
            .Where(organization => organization.Slug == slug)
            .Select(organization => (Guid?)organization.Id)
            .FirstOrDefaultAsync(context.RequestAborted);

        if (organizationId is null)
        {
            _logger.LogWarning("Public request with unknown organization slug {Slug}", slug);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            await context.Response.WriteAsync(
                JsonSerializer.Serialize(new { error = "Организация не найдена." }));
            return;
        }

        organizationContext.SetCurrentOrganizationId(organizationId.Value);
        await _next(context);
    }

    private static bool TryGetOrganizationSlug(PathString path, out string slug)
    {
        slug = string.Empty;
        var value = path.Value;

        if (value is null || !value.StartsWith(PublicApiPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var remainder = value[PublicApiPrefix.Length..];
        var separatorIndex = remainder.IndexOf('/');
        slug = separatorIndex < 0 ? remainder : remainder[..separatorIndex];

        if (string.IsNullOrWhiteSpace(slug))
        {
            return false;
        }

        // Slug хранится в нормализованном (lowercase) виде
        slug = slug.Trim().ToLowerInvariant();
        return true;
    }
}
