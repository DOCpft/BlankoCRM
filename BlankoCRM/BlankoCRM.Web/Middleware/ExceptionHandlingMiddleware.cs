using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Text.Json;

namespace BlankoCRM.Web.Middleware;

/// <summary>
/// Глобальный middleware для обработки необработанных исключений.
/// Возвращает человекочитаемый JSON-ответ вместо 500 с стектрейсом.
/// Помещаем сюда либо частовознгикающие исключения, чтобы сами перехватывались, либо "хз что"
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
        catch (DbUpdateException ex)
        {
            // Ошибки базы данных (нарушение уникальности, внешние ключи и т.д.)
            _logger.LogError(ex, "Database error occurred");

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.Conflict;

            var message = ex.InnerException?.Message ?? ex.Message;

            // Пользователю отдаём общее сообщение, чтобы не светить детали БД
            var response = new
            {
                error = "Произошла ошибка при сохранении данных. Проверьте, что данные уникальны и корректны.",
                detail = message
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
        catch (KeyNotFoundException ex) 
        {
            _logger.LogError($"KeyNotFound exception: {ex.Message}");

            context.Response.ContentType = "application/json";
            context.Response.StatusCode= (int)HttpStatusCode.NotFound;

            var response = new
            {
                error = "Запись с таким ID не найдена в БД. Убедитесь в правильности ID."
            };

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
