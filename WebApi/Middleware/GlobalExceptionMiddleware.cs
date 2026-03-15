using System.Net;
using System.Text.Json;

namespace WebApi.Middleware;

public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);  
        }
        catch (KeyNotFoundException ex)
        {
            logger.LogWarning(ex, "Ресурс не найден: {Path}", context.Request.Path);
            await WriteErrorAsync(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "Нет доступа: {Path}", context.Request.Path);
            await WriteErrorAsync(context, HttpStatusCode.Unauthorized, ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Необработанное исключение на {Method} {Path}",
                context.Request.Method, context.Request.Path);
            await WriteErrorAsync(context, HttpStatusCode.InternalServerError, "Internal server error");
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, HttpStatusCode code, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        var body = JsonSerializer.Serialize(new
        {
            statusCode = (int)code,
            message = message,
            timestamp = DateTime.UtcNow
        });

        await context.Response.WriteAsync(body);
    }
}