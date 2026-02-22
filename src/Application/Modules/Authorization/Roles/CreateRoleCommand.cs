using Application.Abstractions.Messaging;

namespace Application.Authorization.Roles;

public sealed record CreateRoleCommand(string RoleName) : ICommand<RoleCrudResponse>;
