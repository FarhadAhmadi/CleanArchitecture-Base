using Application.Abstractions.Data;
using Domain.Auditing;
using Domain.Authorization;
using Domain.Logging;
using Domain.Todos;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database;

internal sealed class ApplicationReadDbContext(ApplicationDbContext dbContext) : IApplicationReadDbContext
{
    public IQueryable<User> Users => dbContext.Users.AsNoTracking();
    public IQueryable<LogEvent> LogEvents => dbContext.LogEvents.AsNoTracking();
    public IQueryable<AlertRule> AlertRules => dbContext.AlertRules.AsNoTracking();
    public IQueryable<AlertIncident> AlertIncidents => dbContext.AlertIncidents.AsNoTracking();
    public IQueryable<RefreshToken> RefreshTokens => dbContext.RefreshTokens.AsNoTracking();
    public IQueryable<Role> Roles => dbContext.Roles.AsNoTracking();
    public IQueryable<Permission> Permissions => dbContext.Permissions.AsNoTracking();
    public IQueryable<UserRole> UserRoles => dbContext.UserRoles.AsNoTracking();
    public IQueryable<RolePermission> RolePermissions => dbContext.RolePermissions.AsNoTracking();
    public IQueryable<UserPermission> UserPermissions => dbContext.UserPermissions.AsNoTracking();
    public IQueryable<TodoItem> TodoItems => dbContext.TodoItems.AsNoTracking();
    public IQueryable<UserExternalLogin> UserExternalLogins => dbContext.UserExternalLogins.AsNoTracking();
    public IQueryable<AuditEntry> AuditEntries => dbContext.AuditEntries.AsNoTracking();
}
