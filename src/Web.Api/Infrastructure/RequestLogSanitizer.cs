using Microsoft.AspNetCore.Http.Extensions;

namespace Web.Api.Infrastructure;

internal static class RequestLogSanitizer
{
    private static readonly string[] SensitiveKeyFragments =
    [
        "password",
        "token",
        "secret",
        "apikey",
        "api_key",
        "authorization",
        "code",
        "otp",
        "nonce"
    ];

    public static string SanitizeQueryString(HttpRequest request)
    {
        if (!request.QueryString.HasValue || request.Query.Count == 0)
        {
            return string.Empty;
        }

        QueryBuilder queryBuilder = new();

        foreach (KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues> item in request.Query
                     .OrderBy(x => x.Key, StringComparer.Ordinal))
        {
            if (IsSensitive(item.Key))
            {
                queryBuilder.Add(item.Key, "[REDACTED]");
                continue;
            }

            foreach (string value in item.Value)
            {
                queryBuilder.Add(item.Key, Truncate(value ?? string.Empty, 128));
            }
        }

        return queryBuilder.ToQueryString().Value ?? string.Empty;
    }

    private static bool IsSensitive(string key)
    {
        foreach (string fragment in SensitiveKeyFragments)
        {
            if (key.Contains(fragment, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static string Truncate(string value, int maxLength)
    {
        if (value.Length <= maxLength)
        {
            return value;
        }

        return value[..maxLength];
    }
}
