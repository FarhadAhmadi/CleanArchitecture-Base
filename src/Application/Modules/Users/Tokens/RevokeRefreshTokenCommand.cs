using Application.Abstractions.Messaging;

namespace Application.Users.Tokens;

public sealed record RevokeRefreshTokenCommand(string RefreshToken) : ICommand;
