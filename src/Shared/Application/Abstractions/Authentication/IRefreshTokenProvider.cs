namespace Application.Abstractions.Authentication;

public interface IRefreshTokenProvider
{
    string Generate();
    string Hash(string refreshToken);
}
