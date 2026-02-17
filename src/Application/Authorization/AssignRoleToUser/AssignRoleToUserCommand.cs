using Application.Abstractions.Messaging;

namespace Application.Authorization.AssignRoleToUser;

public sealed record AssignRoleToUserCommand(Guid UserId, string RoleName) : ICommand;
