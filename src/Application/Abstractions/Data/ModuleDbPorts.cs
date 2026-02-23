using Domain.Auditing;
using Domain.Authorization;
using Domain.Files;
using Domain.Logging;
using Domain.Modules.Notifications;
using Domain.Modules.Scheduler;
using Domain.Notifications;
using Domain.Profiles;
using Domain.Todos;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Application.Abstractions.Data;

public interface IModuleWriteDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IUsersWriteDbContext : IModuleWriteDbContext
{
    DbSet<User> Users { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<UserPasswordHistory> UserPasswordHistories { get; }
    DbSet<UserExternalLogin> UserExternalLogins { get; }
    DbSet<Role> Roles { get; }
    DbSet<UserRole> UserRoles { get; }
}

public interface IUsersReadDbContext
{
    IQueryable<User> Users { get; }
    IQueryable<RefreshToken> RefreshTokens { get; }
    IQueryable<UserPasswordHistory> UserPasswordHistories { get; }
    IQueryable<UserExternalLogin> UserExternalLogins { get; }
}

public interface IAuthorizationWriteDbContext : IModuleWriteDbContext
{
    DbSet<Role> Roles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<UserRole> UserRoles { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<UserPermission> UserPermissions { get; }
}

public interface IAuthorizationReadDbContext
{
    IQueryable<Role> Roles { get; }
    IQueryable<Permission> Permissions { get; }
    IQueryable<UserRole> UserRoles { get; }
    IQueryable<RolePermission> RolePermissions { get; }
    IQueryable<UserPermission> UserPermissions { get; }
}

public interface ITodosWriteDbContext : IModuleWriteDbContext
{
    DbSet<TodoItem> TodoItems { get; }
    DbSet<User> Users { get; }
}

public interface ITodosReadDbContext
{
    IQueryable<TodoItem> TodoItems { get; }
}

public interface IAuditWriteDbContext : IModuleWriteDbContext
{
    DbSet<AuditEntry> AuditEntries { get; }
}

public interface IAuditReadDbContext
{
    IQueryable<AuditEntry> AuditEntries { get; }
}

public interface ILoggingWriteDbContext : IModuleWriteDbContext
{
    DbSet<LogEvent> LogEvents { get; }
    DbSet<AlertRule> AlertRules { get; }
    DbSet<AlertIncident> AlertIncidents { get; }
    DbSet<Role> Roles { get; }
    DbSet<Permission> Permissions { get; }
    DbSet<RolePermission> RolePermissions { get; }
    DbSet<UserPermission> UserPermissions { get; }
}

public interface ILoggingReadDbContext
{
    IQueryable<LogEvent> LogEvents { get; }
    IQueryable<AlertRule> AlertRules { get; }
    IQueryable<AlertIncident> AlertIncidents { get; }
    IQueryable<Role> Roles { get; }
    IQueryable<Permission> Permissions { get; }
    IQueryable<RolePermission> RolePermissions { get; }
    IQueryable<UserPermission> UserPermissions { get; }
}

public interface IProfilesWriteDbContext : IModuleWriteDbContext
{
    DbSet<UserProfile> UserProfiles { get; }
    DbSet<User> Users { get; }
    DbSet<FileAsset> FileAssets { get; }
}

public interface IProfilesReadDbContext
{
    IQueryable<UserProfile> UserProfiles { get; }
    IQueryable<User> Users { get; }
}

public interface INotificationsWriteDbContext : IModuleWriteDbContext
{
    DbSet<NotificationMessage> NotificationMessages { get; }
    DbSet<NotificationTemplate> NotificationTemplates { get; }
    DbSet<NotificationTemplateRevision> NotificationTemplateRevisions { get; }
    DbSet<NotificationSchedule> NotificationSchedules { get; }
    DbSet<NotificationPermissionEntry> NotificationPermissionEntries { get; }
}

public interface INotificationsReadDbContext
{
    IQueryable<NotificationMessage> NotificationMessages { get; }
    IQueryable<NotificationTemplate> NotificationTemplates { get; }
    IQueryable<NotificationTemplateRevision> NotificationTemplateRevisions { get; }
    IQueryable<NotificationSchedule> NotificationSchedules { get; }
    IQueryable<NotificationPermissionEntry> NotificationPermissionEntries { get; }
    IQueryable<Role> Roles { get; }
    IQueryable<UserRole> UserRoles { get; }
}

public interface IFilesWriteDbContext : IModuleWriteDbContext
{
    DbSet<FileAsset> FileAssets { get; }
    DbSet<FileTag> FileTags { get; }
    DbSet<FileAccessAudit> FileAccessAudits { get; }
    DbSet<FilePermissionEntry> FilePermissionEntries { get; }
}

public interface IFilesReadDbContext
{
    IQueryable<FileAsset> FileAssets { get; }
    IQueryable<FileTag> FileTags { get; }
    IQueryable<FileAccessAudit> FileAccessAudits { get; }
    IQueryable<FilePermissionEntry> FilePermissionEntries { get; }
    IQueryable<Role> Roles { get; }
    IQueryable<UserRole> UserRoles { get; }
}

public interface ISchedulerWriteDbContext : IModuleWriteDbContext
{
    DbSet<ScheduledJob> ScheduledJobs { get; }
    DbSet<JobSchedule> JobSchedules { get; }
    DbSet<JobDependency> JobDependencies { get; }
    DbSet<JobExecution> JobExecutions { get; }
    DbSet<JobPermissionEntry> JobPermissionEntries { get; }
}

public interface ISchedulerReadDbContext
{
    IQueryable<ScheduledJob> ScheduledJobs { get; }
    IQueryable<JobSchedule> JobSchedules { get; }
    IQueryable<JobDependency> JobDependencies { get; }
    IQueryable<JobExecution> JobExecutions { get; }
    IQueryable<JobPermissionEntry> JobPermissionEntries { get; }
}
