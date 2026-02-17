using Domain.Authorization;
using Domain.Logging;
using Domain.Todos;
using Domain.Users;

namespace Application.Abstractions.Data;

public interface IApplicationReadDbContext
{
    IQueryable<User> Users { get; }
    IQueryable<LogEvent> LogEvents { get; }
    IQueryable<AlertRule> AlertRules { get; }
    IQueryable<AlertIncident> AlertIncidents { get; }
    IQueryable<RefreshToken> RefreshTokens { get; }
    IQueryable<Role> Roles { get; }
    IQueryable<Permission> Permissions { get; }
    IQueryable<UserRole> UserRoles { get; }
    IQueryable<RolePermission> RolePermissions { get; }
    IQueryable<UserPermission> UserPermissions { get; }
    IQueryable<TodoItem> TodoItems { get; }
}
