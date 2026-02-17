using System.Text.RegularExpressions;

namespace Infrastructure.Logging;

internal sealed partial class LogSanitizer : ILogSanitizer
{
    public IngestLogRequest Sanitize(IngestLogRequest input)
    {
        input.Message = SanitizeText(input.Message) ?? string.Empty;
        input.SourceService = SanitizeIdentifier(input.SourceService) ?? "Web.Api";
        input.SourceModule = SanitizeIdentifier(input.SourceModule) ?? "unknown";
        input.TraceId = SanitizeIdentifier(input.TraceId);
        input.RequestId = SanitizeIdentifier(input.RequestId);
        input.TenantId = SanitizeIdentifier(input.TenantId);
        input.ActorType = SanitizeIdentifier(input.ActorType) ?? "system";
        input.ActorId = SanitizeIdentifier(input.ActorId);
        input.Outcome = SanitizeIdentifier(input.Outcome) ?? "unknown";
        input.SessionId = SanitizeIdentifier(input.SessionId);
        input.Ip = SanitizeIdentifier(input.Ip);
        input.PayloadJson = SanitizeText(input.PayloadJson);
        input.UserAgent = SanitizeText(input.UserAgent);
        input.DeviceInfo = SanitizeText(input.DeviceInfo);
        input.HttpMethod = SanitizeIdentifier(input.HttpMethod);
        input.HttpPath = SanitizeText(input.HttpPath);
        input.ErrorCode = SanitizeText(input.ErrorCode);
        input.Tags = [.. input.Tags
            .Select(SanitizeText)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!)];

        if (input.HttpStatusCode is < 100 or > 599)
        {
            input.HttpStatusCode = null;
        }

        return input;
    }

    private static string? SanitizeText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        string sanitized = value
            .Replace("\r", " ", StringComparison.Ordinal)
            .Replace("\n", " ", StringComparison.Ordinal)
            .Replace("\t", " ", StringComparison.Ordinal);

        sanitized = PasswordRegex().Replace(sanitized, "password=***");
        sanitized = TokenRegex().Replace(sanitized, "token=***");
        sanitized = CardRegex().Replace(sanitized, "****-****-****-****");
        sanitized = NationalRegex().Replace(sanitized, "***-**-****");
        sanitized = EmailRegex().Replace(sanitized, MaskEmail);

        return sanitized.Trim();
    }

    private static string? SanitizeIdentifier(string? value)
    {
        string? sanitized = SanitizeText(value);
        return string.IsNullOrWhiteSpace(sanitized)
            ? sanitized
            : IdentifierRegex().Replace(sanitized, string.Empty);
    }

    private static string MaskEmail(Match match)
    {
        string email = match.Value;
        int at = email.IndexOf('@');
        if (at <= 1)
        {
            return "***@***";
        }

        string user = email[..at];
        string domain = email[(at + 1)..];
        return $"{user[0]}***@{domain}";
    }

    [GeneratedRegex(@"(?i)(password\s*[:=]\s*)([^,\s]+)")]
    private static partial Regex PasswordRegex();

    [GeneratedRegex(@"(?i)(token\s*[:=]\s*)([^,\s]+)")]
    private static partial Regex TokenRegex();

    [GeneratedRegex(@"\b(?:\d[ -]*?){13,19}\b")]
    private static partial Regex CardRegex();

    [GeneratedRegex(@"\b\d{3}-\d{2}-\d{4}\b")]
    private static partial Regex NationalRegex();

    [GeneratedRegex(@"\b[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}\b", RegexOptions.IgnoreCase)]
    private static partial Regex EmailRegex();

    [GeneratedRegex(@"[^a-zA-Z0-9_\-.:@/\s]")]
    private static partial Regex IdentifierRegex();
}
