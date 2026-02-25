namespace Application.Abstractions.Authentication;

public interface ITokenLifetimeProvider
{
    int AccessTokenExpirationInMinutes { get; }
    int RefreshTokenExpirationInDays { get; }
}
