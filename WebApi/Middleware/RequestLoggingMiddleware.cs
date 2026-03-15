namespace WebApi.Middleware;

public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        logger.LogInformation("▶ {Method} {Path} начат",
            context.Request.Method,
            context.Request.Path);

        await next(context);  

        stopwatch.Stop();

        logger.LogInformation("◀ {Method} {Path} → {StatusCode} за {Ms}ms",
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            stopwatch.ElapsedMilliseconds);
    }
}