using Application.Abstractions.Messaging;

namespace Application.Authorization.Roles;

public sealed record GetRoleByIdQuery(Guid RoleId) : IQuery<RoleCrudResponse>;
