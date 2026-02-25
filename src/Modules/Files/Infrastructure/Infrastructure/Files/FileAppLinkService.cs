using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Files;

public sealed class FileAppLinkService(FileStorageOptions options)
{
    private readonly byte[] _key = SHA256.HashData(Encoding.UTF8.GetBytes(options.AppLinkSigningKey));

    public string CreateToken(Guid fileId, string mode, DateTime nowUtc)
    {
        DateTime expiresAtUtc = nowUtc.AddMinutes(Math.Max(1, options.AppLinkExpiryMinutes));
        long expiresAtUnix = new DateTimeOffset(expiresAtUtc).ToUnixTimeSeconds();
        string payload = $"{fileId:N}|{expiresAtUnix}|{mode}";
        string payloadEncoded = Base64UrlEncode(Encoding.UTF8.GetBytes(payload));
        string signatureEncoded = Base64UrlEncode(Sign(payloadEncoded));
        return $"{payloadEncoded}.{signatureEncoded}";
    }

    public bool TryValidateToken(string token, string expectedMode, DateTime nowUtc, out Guid fileId)
    {
        fileId = Guid.Empty;

        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        string[] parts = token.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
        {
            return false;
        }

        byte[] providedSignature = Base64UrlDecode(parts[1]);
        byte[] computedSignature = Sign(parts[0]);
        if (providedSignature.Length == 0 ||
            computedSignature.Length == 0 ||
            !CryptographicOperations.FixedTimeEquals(providedSignature, computedSignature))
        {
            return false;
        }

        string payload = Encoding.UTF8.GetString(Base64UrlDecode(parts[0]));
        string[] payloadParts = payload.Split('|', StringSplitOptions.TrimEntries);
        if (payloadParts.Length != 3)
        {
            return false;
        }

        if (!Guid.TryParse(payloadParts[0], out Guid parsedFileId))
        {
            return false;
        }

        if (!long.TryParse(payloadParts[1], out long expiresAtUnix))
        {
            return false;
        }

        if (!string.Equals(payloadParts[2], expectedMode, StringComparison.Ordinal))
        {
            return false;
        }

        long nowUnix = new DateTimeOffset(nowUtc).ToUnixTimeSeconds();
        if (expiresAtUnix < nowUnix)
        {
            return false;
        }

        fileId = parsedFileId;
        return true;
    }

    private byte[] Sign(string payloadEncoded)
    {
        using var hmac = new HMACSHA256(_key);
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(payloadEncoded));
    }

    private static string Base64UrlEncode(byte[] value)
    {
        return Convert.ToBase64String(value)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static byte[] Base64UrlDecode(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        string normalized = value
            .Replace('-', '+')
            .Replace('_', '/');

        int pad = 4 - normalized.Length % 4;
        if (pad is > 0 and < 4)
        {
            normalized += new string('=', pad);
        }

        try
        {
            return Convert.FromBase64String(normalized);
        }
        catch
        {
            return [];
        }
    }
}
