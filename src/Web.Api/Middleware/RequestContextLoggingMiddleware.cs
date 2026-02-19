using System.Diagnostics;
using System.Security.Claims;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        string requestPath = context.Request.Path.Value ?? string.Empty;
        string requestMethod = context.Request.Method;
        string queryString = context.Request.QueryString.HasValue ? context.Request.QueryString.Value! : string.Empty;
        string userAgent = context.Request.Headers.UserAgent.ToString();
        string referer = context.Request.Headers.Referer.ToString();
        string endpointName = context.GetEndpoint()?.DisplayName ?? "unknown";
        long startTimestamp = Stopwatch.GetTimestamp();
        ILogger<RequestContextLoggingMiddleware> logger =
            context.RequestServices.GetRequiredService<ILogger<RequestContextLoggingMiddleware>>();

        context.TraceIdentifier = correlationId;
        context.Response.Headers[CorrelationIdHeaderName] = correlationId;

        Activity.Current?.SetTag("correlation.id", correlationId);
        Activity.Current?.SetTag("http.request.method", requestMethod);
        Activity.Current?.SetTag("url.path", requestPath);
        Activity.Current?.SetTag("url.query", queryString);
        Activity.Current?.SetTag("enduser.id", context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous");

        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("RequestPath", requestPath))
        using (LogContext.PushProperty("RequestMethod", requestMethod))
        using (LogContext.PushProperty("RequestQueryString", queryString))
        using (LogContext.PushProperty("RequestHost", context.Request.Host.Value))
        using (LogContext.PushProperty("RequestScheme", context.Request.Scheme))
        using (LogContext.PushProperty("UserAgent", userAgent))
        using (LogContext.PushProperty("Referer", referer))
        using (LogContext.PushProperty("EndpointName", endpointName))
        using (LogContext.PushProperty("RemoteIp", context.Connection.RemoteIpAddress?.ToString() ?? string.Empty))
        using (LogContext.PushProperty("UserId", context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous"))
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("HTTP request started {RequestMethod} {RequestPath}", requestMethod, requestPath);
            }

            await next.Invoke(context);

            double elapsedMs = Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds;
            int statusCode = context.Response.StatusCode;

            if (statusCode >= StatusCodes.Status500InternalServerError)
            {
                logger.LogError(
                    "HTTP request completed with server error {RequestMethod} {RequestPath} {StatusCode} in {ElapsedMs:0.0000} ms",
                    requestMethod,
                    requestPath,
                    statusCode,
                    elapsedMs);
            }
            else if (statusCode >= StatusCodes.Status400BadRequest)
            {
                logger.LogWarning(
                    "HTTP request completed with client error {RequestMethod} {RequestPath} {StatusCode} in {ElapsedMs:0.0000} ms",
                    requestMethod,
                    requestPath,
                    statusCode,
                    elapsedMs);
            }
            else if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "HTTP request completed {RequestMethod} {RequestPath} {StatusCode} in {ElapsedMs:0.0000} ms",
                    requestMethod,
                    requestPath,
                    statusCode,
                    elapsedMs);
            }
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
