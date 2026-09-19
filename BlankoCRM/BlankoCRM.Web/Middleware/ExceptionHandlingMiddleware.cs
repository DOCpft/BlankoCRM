using Microsoft.EntityFrameworkCore;
using BlankoCRM.Core.Exceptions;
using System.Net;
using System.Text.Json;
using Npgsql;

namespace BlankoCRM.Web.Middleware;

/// <summary>
/// Глобальный middleware для обработки необработанных исключений.
/// Возвращает человекочитаемый JSON-ответ вместо 500 со стек-трейсом.
/// Помещаем сюда часто возникающие исключения, чтобы они обрабатывались явно,
/// либо возвращаем общее сообщение об ошибке.
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
            _logger.LogWarning(ex,
                "Domain error {ErrorType}: {Message}",
                ex.GetType().Name, ex.Message);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = ex.StatusCode;
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = ex.Message }));
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt");
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = ex.Message }));
        }
        catch (DbUpdateException ex)
        {
            var sqlState = (ex.InnerException as PostgresException)?.SqlState;
            _logger.LogError(ex, "Database error. SqlState: {SqlState}", sqlState);

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
            // Все остальные необработанные исключения
            _logger.LogError(ex, "Unhandled exception occurred");
            
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = new
            {
                error = "Внутренняя ошибка сервера. Пожалуйста, попробуйте позже."
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}

