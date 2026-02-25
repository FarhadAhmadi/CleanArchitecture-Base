using Application.Abstractions.Authentication;

namespace Infrastructure.Authentication;

internal sealed class TokenLifetimeProvider(
    JwtOptions jwtOptions,
    RefreshTokenOptions refreshTokenOptions) : ITokenLifetimeProvider
{
    public int AccessTokenExpirationInMinutes => jwtOptions.ExpirationInMinutes;

    public int RefreshTokenExpirationInDays => refreshTokenOptions.ExpirationInDays;
}
