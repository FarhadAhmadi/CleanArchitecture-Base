using Web.Api.Infrastructure;

namespace Web.Api.Middleware;

internal sealed class RequestHardeningMiddleware(RequestDelegate next, ApiSecurityOptions options)
{
    private static readonly string[] SupportedBodyContentTypes =
    [
        "application/json",
        "application/problem+json",
        "application/x-www-form-urlencoded",
        "multipart/form-data"
    ];

    public async Task Invoke(HttpContext context)
    {
        if (context.Request.Headers.Count > Math.Max(1, options.MaxRequestHeaderCount))
        {
            await WriteProblemAsync(context, StatusCodes.Status400BadRequest, "Invalid request");
            return;
        }

        if (!HasValidHeaderSize(context.Request.Headers, Math.Max(1, options.MaxRequestHeadersTotalSizeKb) * 1024))
        {
            await WriteProblemAsync(context, StatusCodes.Status431RequestHeaderFieldsTooLarge, "Request headers too large");
            return;
        }

        if (options.EnforceJsonAcceptHeader &&
            !HasJsonCompatibleAccept(context.Request.Headers.Accept))
        {
            await WriteProblemAsync(context, StatusCodes.Status406NotAcceptable, "Not acceptable");
            return;
        }

        if (RequiresBodyContentTypeValidation(context.Request) &&
            !HasSupportedBodyContentType(context.Request.ContentType))
        {
            await WriteProblemAsync(context, StatusCodes.Status415UnsupportedMediaType, "Unsupported media type");
            return;
        }

        await next(context);
    }

    private static bool RequiresBodyContentTypeValidation(HttpRequest request)
    {
        if (!HttpMethods.IsPost(request.Method) &&
            !HttpMethods.IsPut(request.Method) &&
            !HttpMethods.IsPatch(request.Method))
        {
            return false;
        }

        if (request.ContentLength.GetValueOrDefault() <= 0)
        {
            return false;
        }

        return true;
    }

    private static bool HasSupportedBodyContentType(string? contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
        {
            return false;
        }

        return SupportedBodyContentTypes.Any(x => contentType.StartsWith(x, StringComparison.OrdinalIgnoreCase))
               || contentType.StartsWith("application/", StringComparison.OrdinalIgnoreCase) &&
                  contentType.Contains("+json", StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasJsonCompatibleAccept(string? acceptHeader)
    {
        if (string.IsNullOrWhiteSpace(acceptHeader))
        {
            return true;
        }

        return acceptHeader.Contains("application/json", StringComparison.OrdinalIgnoreCase)
               || acceptHeader.Contains("application/problem+json", StringComparison.OrdinalIgnoreCase)
               || acceptHeader.Contains("*/*", StringComparison.OrdinalIgnoreCase)
               || acceptHeader.Contains("application/*", StringComparison.OrdinalIgnoreCase);
    }

    private static bool HasValidHeaderSize(IHeaderDictionary headers, int maxBytes)
    {
        int total = 0;
        foreach (KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues> header in headers)
        {
            total += header.Key.Length;
            foreach (string value in header.Value)
            {
                total += value?.Length ?? 0;
            }

            if (total > maxBytes)
            {
                return false;
            }
        }

        return true;
    }

    private static async Task WriteProblemAsync(HttpContext context, int statusCode, string title)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(new
        {
            type = $"https://httpstatuses.com/{statusCode}",
            title,
            status = statusCode,
            traceId = context.TraceIdentifier
        });
    }
}
