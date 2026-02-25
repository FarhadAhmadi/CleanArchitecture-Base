using Application.Abstractions.Messaging;

namespace Application.Authorization.Roles;

public sealed record UpdateRoleCommand(Guid RoleId, string RoleName) : ICommand<RoleCrudResponse>;
