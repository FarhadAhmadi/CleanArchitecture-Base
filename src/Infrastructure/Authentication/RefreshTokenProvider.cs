using System.Security.Cryptography;
using System.Text;
using Application.Abstractions.Authentication;

namespace Infrastructure.Authentication;

internal sealed class RefreshTokenProvider : IRefreshTokenProvider
{
    public string Generate()
    {
        Span<byte> buffer = stackalloc byte[64];
        RandomNumberGenerator.Fill(buffer);
        return Convert.ToBase64String(buffer);
    }

    public string Hash(string refreshToken)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(refreshToken);
        byte[] hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}
