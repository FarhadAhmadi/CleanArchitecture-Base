using Domain.Auditing;
using Domain.Authorization;
using Domain.Files;
using Domain.Logging;
using Domain.Notifications;
using Domain.Profiles;
using Domain.Todos;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.Abstractions.Data;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<LogEvent> LogEvents { get; }
    DbSet<AlertRule> AlertRules { get; }
    DbSet<AlertIncident> AlertIncidents { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Role> Roles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<UserPermission> UserPermissions { get; }
    DbSet<TodoItem> TodoItems { get; }
    DbSet<UserExternalLogin> UserExternalLogins { get; }
    DbSet<AuditEntry> AuditEntries { get; }
    DbSet<FileAsset> FileAssets { get; }
    DbSet<FileTag> FileTags { get; }
    DbSet<FileAccessAudit> FileAccessAudits { get; }
    DbSet<FilePermissionEntry> FilePermissionEntries { get; }
    DbSet<NotificationMessage> NotificationMessages { get; }
    DbSet<NotificationTemplate> NotificationTemplates { get; }
    DbSet<NotificationTemplateRevision> NotificationTemplateRevisions { get; }
    DbSet<NotificationSchedule> NotificationSchedules { get; }
    DbSet<NotificationPermissionEntry> NotificationPermissionEntries { get; }
    DbSet<NotificationDeliveryAttempt> NotificationDeliveryAttempts { get; }
    DbSet<UserProfile> UserProfiles { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
