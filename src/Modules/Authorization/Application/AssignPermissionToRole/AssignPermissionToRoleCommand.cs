using Application.Abstractions.Messaging;

namespace Application.Authorization.AssignPermissionToRole;

public sealed record AssignPermissionToRoleCommand(string RoleName, string PermissionCode) : ICommand;
