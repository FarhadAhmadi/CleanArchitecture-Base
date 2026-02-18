using Application.Abstractions.Data;
using Domain.Auditing;
using Domain.Authorization;
using Domain.Files;
using Domain.Logging;
using Domain.Notifications;
using Domain.Todos;
using Domain.Users;
using Infrastructure.Integration;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database;

public sealed class ApplicationReadDbContext(DbContextOptions<ApplicationReadDbContext> options)
    : DbContext(options), IApplicationReadDbContext
{
    public IQueryable<User> Users => Set<User>().AsNoTracking();
    public IQueryable<LogEvent> LogEvents => Set<LogEvent>().AsNoTracking();
    public IQueryable<AlertRule> AlertRules => Set<AlertRule>().AsNoTracking();
    public IQueryable<AlertIncident> AlertIncidents => Set<AlertIncident>().AsNoTracking();
    public IQueryable<RefreshToken> RefreshTokens => Set<RefreshToken>().AsNoTracking();
    public IQueryable<Role> Roles => Set<Role>().AsNoTracking();
    public IQueryable<Permission> Permissions => Set<Permission>().AsNoTracking();
    public IQueryable<UserRole> UserRoles => Set<UserRole>().AsNoTracking();
    public IQueryable<RolePermission> RolePermissions => Set<RolePermission>().AsNoTracking();
    public IQueryable<UserPermission> UserPermissions => Set<UserPermission>().AsNoTracking();
    public IQueryable<TodoItem> TodoItems => Set<TodoItem>().AsNoTracking();
    public IQueryable<UserExternalLogin> UserExternalLogins => Set<UserExternalLogin>().AsNoTracking();
    public IQueryable<AuditEntry> AuditEntries => Set<AuditEntry>().AsNoTracking();
    public IQueryable<FileAsset> FileAssets => Set<FileAsset>().AsNoTracking();
    public IQueryable<FileTag> FileTags => Set<FileTag>().AsNoTracking();
    public IQueryable<FileAccessAudit> FileAccessAudits => Set<FileAccessAudit>().AsNoTracking();
    public IQueryable<FilePermissionEntry> FilePermissionEntries => Set<FilePermissionEntry>().AsNoTracking();
    public IQueryable<NotificationMessage> NotificationMessages => Set<NotificationMessage>().AsNoTracking();
    public IQueryable<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>().AsNoTracking();
    public IQueryable<NotificationTemplateRevision> NotificationTemplateRevisions => Set<NotificationTemplateRevision>().AsNoTracking();
    public IQueryable<NotificationSchedule> NotificationSchedules => Set<NotificationSchedule>().AsNoTracking();
    public IQueryable<NotificationPermissionEntry> NotificationPermissionEntries => Set<NotificationPermissionEntry>().AsNoTracking();
    public IQueryable<NotificationDeliveryAttempt> NotificationDeliveryAttempts => Set<NotificationDeliveryAttempt>().AsNoTracking();

    internal IQueryable<OutboxMessage> OutboxMessages => Set<OutboxMessage>().AsNoTracking();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationReadDbContext).Assembly);
        modelBuilder.HasDefaultSchema(Schemas.Default);
    }
}
