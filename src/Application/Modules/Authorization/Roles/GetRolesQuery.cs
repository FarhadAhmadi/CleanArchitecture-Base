using Application.Abstractions.Messaging;

namespace Application.Authorization.Roles;

public sealed record GetRolesQuery(string? Search) : IQuery<List<RoleCrudResponse>>;
