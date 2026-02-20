using System.Security.Cryptography;
using System.Text;

namespace Infrastructure.Notifications;
#pragma warning disable IDE0007, IDE0008

public sealed class NotificationSensitiveDataProtector(NotificationOptions options)
{
    private readonly byte[] _key = ResolveKey(options.SensitiveDataEncryptionKey);

    public string Protect(string plainText)
    {
        if (string.IsNullOrWhiteSpace(plainText))
        {
            return string.Empty;
        }

        using Aes aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        byte[] bytes = Encoding.UTF8.GetBytes(plainText);
        byte[] cipher = encryptor.TransformFinalBlock(bytes, 0, bytes.Length);

        byte[] payload = new byte[aes.IV.Length + cipher.Length];
        Buffer.BlockCopy(aes.IV, 0, payload, 0, aes.IV.Length);
        Buffer.BlockCopy(cipher, 0, payload, aes.IV.Length, cipher.Length);
        return Convert.ToBase64String(payload);
    }

    public string Unprotect(string cipherText)
    {
        if (string.IsNullOrWhiteSpace(cipherText))
        {
            return string.Empty;
        }

        byte[] payload = Convert.FromBase64String(cipherText);
        if (payload.Length <= 16)
        {
            throw new CryptographicException("Encrypted payload is invalid.");
        }

        byte[] iv = payload[..16];
        byte[] cipher = payload[16..];

        using Aes aes = Aes.Create();
        aes.Key = _key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();
        byte[] plain = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
        return Encoding.UTF8.GetString(plain);
    }

    public static string ComputeDeterministicHash(string value)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(value ?? string.Empty));
        return Convert.ToHexString(hash);
    }

    private static byte[] ResolveKey(string configured)
    {
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return SHA256.HashData(Encoding.UTF8.GetBytes(configured));
        }

        return SHA256.HashData(Encoding.UTF8.GetBytes("notification-default-key-change-me"));
    }
}
#pragma warning restore IDE0007, IDE0008
