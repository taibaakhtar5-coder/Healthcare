using HealthcareCRM.Services;

namespace HealthcareCRM.Middleware;

public class AuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditMiddleware> _logger;

    public AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IAuditService auditService)
    {
        await _next(context);

        // Log write operations
        var method = context.Request.Method;
        if (method == HttpMethods.Post || method == HttpMethods.Put || method == HttpMethods.Delete)
        {
            var userId   = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
            var userName = context.User?.Identity?.Name ?? "anonymous";
            var path     = context.Request.Path.Value ?? "/";
            var ip       = context.Connection.RemoteIpAddress?.ToString();

            _logger.LogInformation("User {User} performed {Method} on {Path}", userName, method, path);
        }
    }
}
