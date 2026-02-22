using Application.Abstractions.Messaging;

namespace Application.Authorization.Roles;

public sealed record DeleteRoleCommand(Guid RoleId) : ICommand;
