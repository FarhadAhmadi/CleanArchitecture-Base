using Application.Abstractions.Messaging;

namespace Application.Users.Management;

public sealed record DeleteUserCommand(Guid UserId) : ICommand;
