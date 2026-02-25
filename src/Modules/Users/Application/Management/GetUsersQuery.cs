using Application.Abstractions.Messaging;

namespace Application.Users.Management;

public sealed record GetUsersQuery(string? Search) : IQuery<List<UserAdminResponse>>;
