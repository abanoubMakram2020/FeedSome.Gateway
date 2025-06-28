using Microsoft.Extensions.Logging;

namespace FeedSome.Gateway.Middleware;

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LoggingMiddleware> _logger;

    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var startTime = DateTime.UtcNow;
        var originalBodyStream = context.Response.Body;

        try
        {
            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            // Log the incoming request
            _logger.LogInformation(
                "Incoming request: {Method} {Path} from {IP} with User-Agent: {UserAgent}",
                context.Request.Method,
                context.Request.Path,
                GetClientIpAddress(context),
                context.Request.Headers.UserAgent.ToString());

            await _next(context);

            memoryStream.Position = 0;
            await memoryStream.CopyToAsync(originalBodyStream);

            var duration = DateTime.UtcNow - startTime;
            _logger.LogInformation(
                "Request completed: {Method} {Path} - Status: {StatusCode} - Duration: {Duration}ms",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                duration.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Unhandled exception during request: {Method} {Path}",
                context.Request.Method,
                context.Request.Path);

            // Restore the original response body
            context.Response.Body = originalBodyStream;

            // Set error response
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";
            
            var errorResponse = new
            {
                error = "An internal server error occurred",
                timestamp = DateTime.UtcNow,
                path = context.Request.Path
            };

            await context.Response.WriteAsJsonAsync(errorResponse);
        }
    }

    private string GetClientIpAddress(HttpContext context)
    {
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }

        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
    }
} 