namespace Infrastructure.Authentication;

internal sealed class RefreshTokenOptions
{
    public const string SectionName = "RefreshToken";

    public int ExpirationInDays { get; init; } = 14;
}
