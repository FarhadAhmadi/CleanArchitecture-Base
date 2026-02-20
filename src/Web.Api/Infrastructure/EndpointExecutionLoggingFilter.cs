using System.Diagnostics;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Web.Api.Infrastructure;

public static class EndpointExecutionLoggingFilter
{
    public static EndpointFilterDelegate Create(EndpointFilterFactoryContext context, EndpointFilterDelegate next)
    {
        string handlerName = $"{context.MethodInfo.DeclaringType?.Name ?? "Unknown"}.{context.MethodInfo.Name}";

        return async invocationContext =>
        {
            HttpContext httpContext = invocationContext.HttpContext;
            ILogger logger = httpContext.RequestServices
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("Web.Api.EndpointExecution");

            string requestMethod = httpContext.Request.Method;
            string requestPath = httpContext.Request.Path.Value ?? string.Empty;
            string endpointName = httpContext.GetEndpoint()?.DisplayName ?? handlerName;
            long startTimestamp = Stopwatch.GetTimestamp();

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Endpoint execution started {EndpointName} {RequestMethod} {RequestPath}",
                    endpointName,
                    requestMethod,
                    requestPath);
            }

            object? result = await next(invocationContext);

            int statusCode = ResolveStatusCode(httpContext, result);
            double elapsedMs = Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds;

            if (statusCode >= StatusCodes.Status500InternalServerError)
            {
                logger.LogError(
                    "Endpoint execution completed with server error {EndpointName} {RequestMethod} {RequestPath} {StatusCode} in {ElapsedMs:0.0000} ms",
                    endpointName,
                    requestMethod,
                    requestPath,
                    statusCode,
                    elapsedMs);
            }
            else if (statusCode >= StatusCodes.Status400BadRequest)
            {
                logger.LogWarning(
                    "Endpoint execution completed with client error {EndpointName} {RequestMethod} {RequestPath} {StatusCode} in {ElapsedMs:0.0000} ms",
                    endpointName,
                    requestMethod,
                    requestPath,
                    statusCode,
                    elapsedMs);
            }
            else if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation(
                    "Endpoint execution completed {EndpointName} {RequestMethod} {RequestPath} {StatusCode} in {ElapsedMs:0.0000} ms",
                    endpointName,
                    requestMethod,
                    requestPath,
                    statusCode,
                    elapsedMs);
            }

            return result;
        };
    }

    private static int ResolveStatusCode(HttpContext httpContext, object? result)
    {
        if (httpContext.Response.StatusCode is >= 100 and <= 999)
        {
            return httpContext.Response.StatusCode;
        }

        if (result is IStatusCodeHttpResult statusCodeResult && statusCodeResult.StatusCode.HasValue)
        {
            return statusCodeResult.StatusCode.Value;
        }

        return StatusCodes.Status200OK;
    }
}

