using System.Diagnostics;
using System.Security.Claims;
using System.Linq;
using Microsoft.Extensions.Primitives;
using Serilog.Context;

namespace Web.Api.Middleware;

public sealed class RequestContextLoggingMiddleware(RequestDelegate next)
{
    private const string CorrelationIdHeaderName = "X-Correlation-Id";
    private const string LegacyCorrelationIdHeaderName = "Correlation-Id";
    private const int MaxCorrelationIdLength = 64;

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
        string? correlationId = null;
        if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out StringValues primaryHeader))
        {
            correlationId = primaryHeader.FirstOrDefault();
        }
        else if (context.Request.Headers.TryGetValue(LegacyCorrelationIdHeaderName, out StringValues legacyHeader))
        {
            correlationId = legacyHeader.FirstOrDefault();
        }

        string candidate = string.IsNullOrWhiteSpace(correlationId) ? context.TraceIdentifier : correlationId;
        string normalized = candidate.Trim();
        if (normalized.Length > MaxCorrelationIdLength)
        {
            normalized = normalized[..MaxCorrelationIdLength];
        }

        normalized = new string(normalized.Where(ch => char.IsLetterOrDigit(ch) || ch is '-' or '_' or '.').ToArray());
        return string.IsNullOrWhiteSpace(normalized) ? context.TraceIdentifier : normalized;
    }
}
