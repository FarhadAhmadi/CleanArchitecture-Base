using Application.Abstractions.Messaging;
using Application.Abstractions.Authentication;

namespace Application.Users.Tokens;

public sealed record RefreshTokenCommand(string RefreshToken) : ICommand<TokenResponse>;
