using System.Text.RegularExpressions;

namespace Application.Shared;

internal static partial class InputSanitizer
{
    internal static string? SanitizeText(string? value, int maxLength = 500)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        string sanitized = value
            .Replace("\r", " ", StringComparison.Ordinal)
            .Replace("\n", " ", StringComparison.Ordinal)
            .Replace("\t", " ", StringComparison.Ordinal)
            .Trim();

        sanitized = ScriptTagRegex().Replace(sanitized, string.Empty);
        sanitized = SqlCommentRegex().Replace(sanitized, string.Empty);

        return sanitized.Length > maxLength ? sanitized[..maxLength] : sanitized;
    }

    internal static string? SanitizeIdentifier(string? value, int maxLength = 150)
    {
        string? sanitized = SanitizeText(value, maxLength);
        if (string.IsNullOrWhiteSpace(sanitized))
        {
            return sanitized;
        }

        return IdentifierRegex().Replace(sanitized, string.Empty);
    }

    internal static string? SanitizeEmail(string? value)
    {
        return SanitizeText(value, 320);
    }

    internal static List<string> SanitizeList(IEnumerable<string> values, int maxItemLength = 100)
    {
        return [.. values
            .Select(value => SanitizeIdentifier(value, maxItemLength))
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(value => value!)];
    }

    [GeneratedRegex(@"<\s*script[^>]*>.*?<\s*/\s*script\s*>", RegexOptions.IgnoreCase)]
    private static partial Regex ScriptTagRegex();

    [GeneratedRegex(@"(--|/\*|\*/|;)", RegexOptions.IgnoreCase)]
    private static partial Regex SqlCommentRegex();

    [GeneratedRegex(@"[^a-zA-Z0-9_\-.:@/\s]")]
    private static partial Regex IdentifierRegex();
}
