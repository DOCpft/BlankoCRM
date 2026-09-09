using Microsoft.EntityFrameworkCore;
using BlankoCRM.Core.Exceptions;
using System.Net;
using System.Text.Json;
using Npgsql;

namespace BlankoCRM.Web.Middleware;

/// <summary>
/// Р“Р»РѕР±Р°Р»СЊРЅС‹Р№ middleware РґР»СЏ РѕР±СЂР°Р±РѕС‚РєРё РЅРµРѕР±СЂР°Р±РѕС‚Р°РЅРЅС‹С… РёСЃРєР»СЋС‡РµРЅРёР№.
/// Р’РѕР·РІСЂР°С‰Р°РµС‚ С‡РµР»РѕРІРµРєРѕС‡РёС‚Р°РµРјС‹Р№ JSON-РѕС‚РІРµС‚ РІРјРµСЃС‚Рѕ 500 СЃ СЃС‚РµРєС‚СЂРµР№СЃРѕРј.
/// РџРѕРјРµС‰Р°РµРј СЃСЋРґР° Р»РёР±Рѕ С‡Р°СЃС‚РѕРІРѕР·РЅРіРёРєР°СЋС‰РёРµ РёСЃРєР»СЋС‡РµРЅРёСЏ, С‡С‚РѕР±С‹ СЃР°РјРё РїРµСЂРµС…РІР°С‚С‹РІР°Р»РёСЃСЊ, Р»РёР±Рѕ "С…Р· С‡С‚Рѕ"
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain error occurred");
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = ex.StatusCode;
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = ex.Message }));
        }
        catch (UnauthorizedAccessException ex)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = ex.Message }));
        }
        catch (DbUpdateException ex)
        {
            // РћС€РёР±РєРё Р±Р°Р·С‹ РґР°РЅРЅС‹С… (РЅР°СЂСѓС€РµРЅРёРµ СѓРЅРёРєР°Р»СЊРЅРѕСЃС‚Рё, РІРЅРµС€РЅРёРµ РєР»СЋС‡Рё Рё С‚.Рґ.)
            _logger.LogError(ex, "Database error occurred");
            _logger.LogError(
                "Database error details. InnerException: {InnerException}",
                ex.InnerException?.ToString() ?? "<none>");

            context.Response.ContentType = "application/json";
            var postgresException = ex.InnerException as PostgresException;
            var (statusCode, message) = postgresException?.SqlState switch
            {
                PostgresErrorCodes.UniqueViolation => ((int)HttpStatusCode.Conflict, "A record with the same unique value already exists."),
                PostgresErrorCodes.ForeignKeyViolation => ((int)HttpStatusCode.BadRequest, "The request references a missing related record."),
                PostgresErrorCodes.CheckViolation => ((int)HttpStatusCode.BadRequest, "The data violates a database constraint."),
                _ => ((int)HttpStatusCode.InternalServerError, "The database operation failed.")
            };

            context.Response.StatusCode = statusCode;
            var response = new { error = message };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
        catch (Exception ex)
        {
            // Р’СЃРµ РѕСЃС‚Р°Р»СЊРЅС‹Рµ РЅРµРѕР±СЂР°Р±РѕС‚Р°РЅРЅС‹Рµ РёСЃРєР»СЋС‡РµРЅРёСЏ
            _logger.LogError(ex, "Unhandled exception occurred");
            
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = new
            {
                error = "Р’РЅСѓС‚СЂРµРЅРЅСЏСЏ РѕС€РёР±РєР° СЃРµСЂРІРµСЂР°. РџРѕР¶Р°Р»СѓР№СЃС‚Р°, РїРѕРїСЂРѕР±СѓР№С‚Рµ РїРѕР·Р¶Рµ."
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}

