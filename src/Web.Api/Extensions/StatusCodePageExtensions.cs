using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;

namespace Web.Api.Extensions;

internal static class StatusCodePageExtensions
{
    internal static Task WriteProblemDetails(StatusCodeContext context)
    {
        HttpContext httpContext = context.HttpContext;
        int statusCode = httpContext.Response.StatusCode;

        if (statusCode is not StatusCodes.Status401Unauthorized and not StatusCodes.Status403Forbidden and not StatusCodes.Status429TooManyRequests)
        {
            return Task.CompletedTask;
        }

        var payload = new
        {
            type = statusCode switch
            {
                StatusCodes.Status401Unauthorized => "https://httpstatuses.com/401",
                StatusCodes.Status403Forbidden => "https://httpstatuses.com/403",
                StatusCodes.Status429TooManyRequests => "https://httpstatuses.com/429",
                _ => "https://httpstatuses.com/500"
            },
            title = statusCode switch
            {
                StatusCodes.Status401Unauthorized => "Unauthorized",
                StatusCodes.Status403Forbidden => "Forbidden",
                StatusCodes.Status429TooManyRequests => "Too Many Requests",
                _ => "Error"
            },
            status = statusCode,
            traceId = httpContext.TraceIdentifier
        };

        httpContext.Response.ContentType = "application/problem+json";

        return httpContext.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
