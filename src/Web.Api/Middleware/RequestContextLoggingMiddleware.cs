using System.Diagnostics;
using System.Security.Claims;
using Microsoft.Extensions.Primitives;
using Serilog.Context;

namespace Web.Api.Middleware;

public sealed class RequestContextLoggingMiddleware(RequestDelegate next)
{
    private const string CorrelationIdHeaderName = "Correlation-Id";

    public async Task Invoke(HttpContext context)
    {
        string correlationId = GetCorrelationId(context);

        context.TraceIdentifier = correlationId;
        context.Response.Headers[CorrelationIdHeaderName] = correlationId;

        Activity.Current?.SetTag("correlation.id", correlationId);

        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("RequestPath", context.Request.Path.Value ?? string.Empty))
        using (LogContext.PushProperty("RequestMethod", context.Request.Method))
        using (LogContext.PushProperty("RemoteIp", context.Connection.RemoteIpAddress?.ToString() ?? string.Empty))
        using (LogContext.PushProperty("UserId", context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous"))
        {
            await next.Invoke(context);
        }
    }

    private static string GetCorrelationId(HttpContext context)
    {
        context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out StringValues correlationId);
        return correlationId.FirstOrDefault() ?? context.TraceIdentifier;
    }
}
