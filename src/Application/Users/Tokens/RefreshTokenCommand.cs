using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;

namespace Application.Users.Tokens;

public sealed record RefreshTokenCommand(string RefreshToken) : ICommand<TokenResponse>;
