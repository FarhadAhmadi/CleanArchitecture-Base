using Application.Abstractions.Data;
using Application.Abstractions.Authentication;
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
using Infrastructure.Integration;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Infrastructure.Database;

internal static class ModuleModelBuilderExtensions
{
    internal static void ApplyModuleConfigurations(this ModelBuilder modelBuilder, params string[] namespacePrefixes)
    {
        _ = namespacePrefixes;
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}

public abstract class ModuleWriteDbContextBase<TContext>(
    DbContextOptions<TContext> options,
    IntegrationEventSerializer integrationEventSerializer,
    IUserContext? userContext = null) : DbContext(options)
    where TContext : DbContext
{
    private readonly IntegrationEventSerializer _integrationEventSerializer = integrationEventSerializer;
    private readonly IUserContext? _userContext = userContext;

    internal DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditFields();

        List<IDomainEvent> domainEvents = CollectDomainEvents();
        if (domainEvents.Count != 0)
        {
            List<OutboxMessage> outboxMessages = [.. domainEvents.Select(_integrationEventSerializer.ToOutboxMessage)];
            OutboxMessages.AddRange(outboxMessages);
        }

        int result = await base.SaveChangesAsync(cancellationToken);
        ClearDomainEvents();

        return result;
    }

    private void ApplyAuditFields()
    {
        DateTime now = DateTime.UtcNow;
        string actor = ResolveActor();

        foreach (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Entity> entry in ChangeTracker.Entries<Entity>())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.AuditCreatedAtUtc == default)
                {
                    entry.Entity.AuditCreatedAtUtc = now;
                }

                entry.Entity.AuditCreatedBy ??= actor;
                continue;
            }

            if (entry.State != EntityState.Modified)
            {
                continue;
            }

            entry.Property(x => x.AuditCreatedAtUtc).IsModified = false;
            entry.Property(x => x.AuditCreatedBy).IsModified = false;
            entry.Entity.AuditUpdatedAtUtc = now;
            entry.Entity.AuditUpdatedBy = actor;
        }

        foreach (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<User> entry in ChangeTracker.Entries<User>())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.AuditCreatedAtUtc == default)
                {
                    entry.Entity.AuditCreatedAtUtc = now;
                }

                entry.Entity.AuditCreatedBy ??= actor;
                continue;
            }

            if (entry.State != EntityState.Modified)
            {
                continue;
            }

            entry.Property(x => x.AuditCreatedAtUtc).IsModified = false;
            entry.Property(x => x.AuditCreatedBy).IsModified = false;
            entry.Entity.AuditUpdatedAtUtc = now;
            entry.Entity.AuditUpdatedBy = actor;
        }

        foreach (Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Role> entry in ChangeTracker.Entries<Role>())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.AuditCreatedAtUtc == default)
                {
                    entry.Entity.AuditCreatedAtUtc = now;
                }

                entry.Entity.AuditCreatedBy ??= actor;
                continue;
            }

            if (entry.State != EntityState.Modified)
            {
                continue;
            }

            entry.Property(x => x.AuditCreatedAtUtc).IsModified = false;
            entry.Property(x => x.AuditCreatedBy).IsModified = false;
            entry.Entity.AuditUpdatedAtUtc = now;
            entry.Entity.AuditUpdatedBy = actor;
        }
    }

    private string ResolveActor()
    {
        try
        {
            return _userContext?.UserId.ToString("N") ?? "system";
        }
        catch
        {
            return "system";
        }
    }

    private List<IDomainEvent> CollectDomainEvents()
    {
        List<IDomainEvent> entityEvents = [.. ChangeTracker
            .Entries<Entity>()
            .Select(entry => entry.Entity)
            .SelectMany(entity => entity.DomainEvents)];

        List<IDomainEvent> userEvents = [.. ChangeTracker
            .Entries<User>()
            .Select(entry => entry.Entity)
            .SelectMany(user => user.DomainEvents)];

        entityEvents.AddRange(userEvents);
        return entityEvents;
    }

    private void ClearDomainEvents()
    {
        foreach (Entity entity in ChangeTracker.Entries<Entity>().Select(entry => entry.Entity))
        {
            entity.ClearDomainEvents();
        }

        foreach (User user in ChangeTracker.Entries<User>().Select(entry => entry.Entity))
        {
            user.ClearDomainEvents();
        }
    }
}

internal sealed class UsersWriteDbContext(
    DbContextOptions<UsersWriteDbContext> options,
    IntegrationEventSerializer integrationEventSerializer,
    IUserContext? userContext = null)
    : ModuleWriteDbContextBase<UsersWriteDbContext>(options, integrationEventSerializer, userContext), IUsersWriteDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<UserPasswordHistory> UserPasswordHistories => Set<UserPasswordHistory>();
    public DbSet<UserExternalLogin> UserExternalLogins => Set<UserExternalLogin>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Default);
        modelBuilder.ApplyModuleConfigurations(
            "Infrastructure.Database.Configurations.Authentication",
            "Infrastructure.Database.Configurations.Users",
            "Infrastructure.Database.Configurations.Authorization",
            "Infrastructure.Database.Configurations.Integration");
    }
}

internal sealed class UsersReadDbContext(DbContextOptions<UsersReadDbContext> options)
    : DbContext(options), IUsersReadDbContext
{
    public IQueryable<User> Users => Set<User>().AsNoTracking();
    public IQueryable<RefreshToken> RefreshTokens => Set<RefreshToken>().AsNoTracking();
    public IQueryable<UserPasswordHistory> UserPasswordHistories => Set<UserPasswordHistory>().AsNoTracking();
    public IQueryable<UserExternalLogin> UserExternalLogins => Set<UserExternalLogin>().AsNoTracking();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Default);
        modelBuilder.ApplyModuleConfigurations(
            "Infrastructure.Database.Configurations.Authentication",
            "Infrastructure.Database.Configurations.Users");
    }
}

public sealed class AuthorizationWriteDbContext(
    DbContextOptions<AuthorizationWriteDbContext> options,
    IntegrationEventSerializer integrationEventSerializer,
    IUserContext? userContext = null)
    : ModuleWriteDbContextBase<AuthorizationWriteDbContext>(options, integrationEventSerializer, userContext), IAuthorizationWriteDbContext
{
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserPermission> UserPermissions => Set<UserPermission>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Default);
        modelBuilder.ApplyModuleConfigurations(
            "Infrastructure.Database.Configurations.Authorization",
            "Infrastructure.Database.Configurations.Users",
            "Infrastructure.Database.Configurations.Authentication",
            "Infrastructure.Database.Configurations.Integration");
    }
}

internal sealed class AuthorizationReadDbContext(DbContextOptions<AuthorizationReadDbContext> options)
    : DbContext(options), IAuthorizationReadDbContext
{
    public IQueryable<Role> Roles => Set<Role>().AsNoTracking();
    public IQueryable<Permission> Permissions => Set<Permission>().AsNoTracking();
    public IQueryable<UserRole> UserRoles => Set<UserRole>().AsNoTracking();
    public IQueryable<RolePermission> RolePermissions => Set<RolePermission>().AsNoTracking();
    public IQueryable<UserPermission> UserPermissions => Set<UserPermission>().AsNoTracking();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Default);
        modelBuilder.ApplyModuleConfigurations("Infrastructure.Database.Configurations.Authorization");
    }
}

internal sealed class TodosWriteDbContext(
    DbContextOptions<TodosWriteDbContext> options,
    IntegrationEventSerializer integrationEventSerializer,
    IUserContext? userContext = null)
    : ModuleWriteDbContextBase<TodosWriteDbContext>(options, integrationEventSerializer, userContext), ITodosWriteDbContext
{
    public DbSet<TodoItem> TodoItems => Set<TodoItem>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Default);
        modelBuilder.ApplyModuleConfigurations(
            "Infrastructure.Database.Configurations.Todos",
            "Infrastructure.Database.Configurations.Users",
            "Infrastructure.Database.Configurations.Authentication",
            "Infrastructure.Database.Configurations.Integration");
    }
}

internal sealed class TodosReadDbContext(DbContextOptions<TodosReadDbContext> options)
    : DbContext(options), ITodosReadDbContext
{
    public IQueryable<TodoItem> TodoItems => Set<TodoItem>().AsNoTracking();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Default);
        modelBuilder.ApplyModuleConfigurations("Infrastructure.Database.Configurations.Todos");
    }
}

internal sealed class AuditWriteDbContext(
    DbContextOptions<AuditWriteDbContext> options,
    IntegrationEventSerializer integrationEventSerializer,
    IUserContext? userContext = null)
    : ModuleWriteDbContextBase<AuditWriteDbContext>(options, integrationEventSerializer, userContext), IAuditWriteDbContext
{
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Default);
        modelBuilder.ApplyModuleConfigurations(
            "Infrastructure.Database.Configurations.Auditing",
            "Infrastructure.Database.Configurations.Integration");
    }
}

internal sealed class AuditReadDbContext(DbContextOptions<AuditReadDbContext> options)
    : DbContext(options), IAuditReadDbContext
{
    public IQueryable<AuditEntry> AuditEntries => Set<AuditEntry>().AsNoTracking();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Default);
        modelBuilder.ApplyModuleConfigurations("Infrastructure.Database.Configurations.Auditing");
    }
}

internal sealed class LoggingWriteDbContext(
    DbContextOptions<LoggingWriteDbContext> options,
    IntegrationEventSerializer integrationEventSerializer,
    IUserContext? userContext = null)
    : ModuleWriteDbContextBase<LoggingWriteDbContext>(options, integrationEventSerializer, userContext), ILoggingWriteDbContext
{
    public DbSet<LogEvent> LogEvents => Set<LogEvent>();
    public DbSet<AlertRule> AlertRules => Set<AlertRule>();
    public DbSet<AlertIncident> AlertIncidents => Set<AlertIncident>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserPermission> UserPermissions => Set<UserPermission>();
    public DbSet<User> Users => Set<User>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Default);
        modelBuilder.ApplyModuleConfigurations(
            "Infrastructure.Database.Configurations.Logging",
            "Infrastructure.Database.Configurations.Authorization",
            "Infrastructure.Database.Configurations.Users",
            "Infrastructure.Database.Configurations.Authentication",
            "Infrastructure.Database.Configurations.Integration");
    }
}

internal sealed class LoggingReadDbContext(DbContextOptions<LoggingReadDbContext> options)
    : DbContext(options), ILoggingReadDbContext
{
    public IQueryable<LogEvent> LogEvents => Set<LogEvent>().AsNoTracking();
    public IQueryable<AlertRule> AlertRules => Set<AlertRule>().AsNoTracking();
    public IQueryable<AlertIncident> AlertIncidents => Set<AlertIncident>().AsNoTracking();
    public IQueryable<Role> Roles => Set<Role>().AsNoTracking();
    public IQueryable<Permission> Permissions => Set<Permission>().AsNoTracking();
    public IQueryable<RolePermission> RolePermissions => Set<RolePermission>().AsNoTracking();
    public IQueryable<UserPermission> UserPermissions => Set<UserPermission>().AsNoTracking();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Default);
        modelBuilder.ApplyModuleConfigurations(
            "Infrastructure.Database.Configurations.Logging",
            "Infrastructure.Database.Configurations.Authorization");
    }
}

internal sealed class ProfilesWriteDbContext(
    DbContextOptions<ProfilesWriteDbContext> options,
    IntegrationEventSerializer integrationEventSerializer,
    IUserContext? userContext = null)
    : ModuleWriteDbContextBase<ProfilesWriteDbContext>(options, integrationEventSerializer, userContext), IProfilesWriteDbContext
{
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<User> Users => Set<User>();
    public DbSet<FileAsset> FileAssets => Set<FileAsset>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Default);
        modelBuilder.ApplyModuleConfigurations(
            "Infrastructure.Database.Configurations.Profiles",
            "Infrastructure.Database.Configurations.Users",
            "Infrastructure.Database.Configurations.Authentication",
            "Infrastructure.Database.Configurations.Files",
            "Infrastructure.Database.Configurations.Integration");
    }
}

internal sealed class ProfilesReadDbContext(DbContextOptions<ProfilesReadDbContext> options)
    : DbContext(options), IProfilesReadDbContext
{
    public IQueryable<UserProfile> UserProfiles => Set<UserProfile>().AsNoTracking();
    public IQueryable<User> Users => Set<User>().AsNoTracking();
    public IQueryable<FileAsset> FileAssets => Set<FileAsset>().AsNoTracking();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Default);
        modelBuilder.ApplyModuleConfigurations(
            "Infrastructure.Database.Configurations.Profiles",
            "Infrastructure.Database.Configurations.Users",
            "Infrastructure.Database.Configurations.Authentication",
            "Infrastructure.Database.Configurations.Files");
    }
}

public sealed class NotificationsWriteDbContext(
    DbContextOptions<NotificationsWriteDbContext> options,
    IntegrationEventSerializer integrationEventSerializer,
    IUserContext? userContext = null)
    : ModuleWriteDbContextBase<NotificationsWriteDbContext>(options, integrationEventSerializer, userContext), INotificationsWriteDbContext
{
    public DbSet<NotificationMessage> NotificationMessages => Set<NotificationMessage>();
    public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();
    public DbSet<NotificationTemplateRevision> NotificationTemplateRevisions => Set<NotificationTemplateRevision>();
    public DbSet<NotificationSchedule> NotificationSchedules => Set<NotificationSchedule>();
    public DbSet<NotificationPermissionEntry> NotificationPermissionEntries => Set<NotificationPermissionEntry>();
    public DbSet<NotificationDeliveryAttempt> NotificationDeliveryAttempts => Set<NotificationDeliveryAttempt>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Default);
        modelBuilder.ApplyModuleConfigurations(
            "Infrastructure.Database.Configurations.Notifications",
            "Infrastructure.Database.Configurations.Authorization",
            "Infrastructure.Database.Configurations.Users",
            "Infrastructure.Database.Configurations.Authentication",
            "Infrastructure.Database.Configurations.Integration");
    }
}

internal sealed class NotificationsReadDbContext(DbContextOptions<NotificationsReadDbContext> options)
    : DbContext(options), INotificationsReadDbContext
{
    public IQueryable<NotificationMessage> NotificationMessages => Set<NotificationMessage>().AsNoTracking();
    public IQueryable<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>().AsNoTracking();
    public IQueryable<NotificationTemplateRevision> NotificationTemplateRevisions => Set<NotificationTemplateRevision>().AsNoTracking();
    public IQueryable<NotificationSchedule> NotificationSchedules => Set<NotificationSchedule>().AsNoTracking();
    public IQueryable<NotificationPermissionEntry> NotificationPermissionEntries => Set<NotificationPermissionEntry>().AsNoTracking();
    public IQueryable<Role> Roles => Set<Role>().AsNoTracking();
    public IQueryable<UserRole> UserRoles => Set<UserRole>().AsNoTracking();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Default);
        modelBuilder.ApplyModuleConfigurations(
            "Infrastructure.Database.Configurations.Notifications",
            "Infrastructure.Database.Configurations.Authorization");
    }
}

internal sealed class FilesWriteDbContext(
    DbContextOptions<FilesWriteDbContext> options,
    IntegrationEventSerializer integrationEventSerializer,
    IUserContext? userContext = null)
    : ModuleWriteDbContextBase<FilesWriteDbContext>(options, integrationEventSerializer, userContext), IFilesWriteDbContext
{
    public DbSet<FileAsset> FileAssets => Set<FileAsset>();
    public DbSet<FileTag> FileTags => Set<FileTag>();
    public DbSet<FileAccessAudit> FileAccessAudits => Set<FileAccessAudit>();
    public DbSet<FilePermissionEntry> FilePermissionEntries => Set<FilePermissionEntry>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Default);
        modelBuilder.ApplyModuleConfigurations(
            "Infrastructure.Database.Configurations.Files",
            "Infrastructure.Database.Configurations.Authorization",
            "Infrastructure.Database.Configurations.Integration");
    }
}

internal sealed class FilesReadDbContext(DbContextOptions<FilesReadDbContext> options)
    : DbContext(options), IFilesReadDbContext
{
    public IQueryable<FileAsset> FileAssets => Set<FileAsset>().AsNoTracking();
    public IQueryable<FileTag> FileTags => Set<FileTag>().AsNoTracking();
    public IQueryable<FileAccessAudit> FileAccessAudits => Set<FileAccessAudit>().AsNoTracking();
    public IQueryable<FilePermissionEntry> FilePermissionEntries => Set<FilePermissionEntry>().AsNoTracking();
    public IQueryable<Role> Roles => Set<Role>().AsNoTracking();
    public IQueryable<UserRole> UserRoles => Set<UserRole>().AsNoTracking();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Default);
        modelBuilder.ApplyModuleConfigurations(
            "Infrastructure.Database.Configurations.Files",
            "Infrastructure.Database.Configurations.Authorization");
    }
}

public sealed class SchedulerWriteDbContext(
    DbContextOptions<SchedulerWriteDbContext> options,
    IntegrationEventSerializer integrationEventSerializer,
    IUserContext? userContext = null)
    : ModuleWriteDbContextBase<SchedulerWriteDbContext>(options, integrationEventSerializer, userContext), ISchedulerWriteDbContext
{
    public DbSet<ScheduledJob> ScheduledJobs => Set<ScheduledJob>();
    public DbSet<JobSchedule> JobSchedules => Set<JobSchedule>();
    public DbSet<JobDependency> JobDependencies => Set<JobDependency>();
    public DbSet<JobExecution> JobExecutions => Set<JobExecution>();
    public DbSet<JobPermissionEntry> JobPermissionEntries => Set<JobPermissionEntry>();
    public DbSet<SchedulerLockLease> SchedulerLockLeases => Set<SchedulerLockLease>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Default);
        modelBuilder.ApplyModuleConfigurations(
            "Infrastructure.Database.Configurations.Scheduler",
            "Infrastructure.Database.Configurations.Users",
            "Infrastructure.Database.Configurations.Authentication",
            "Infrastructure.Database.Configurations.Integration");
    }
}

internal sealed class SchedulerReadDbContext(DbContextOptions<SchedulerReadDbContext> options)
    : DbContext(options), ISchedulerReadDbContext
{
    public IQueryable<ScheduledJob> ScheduledJobs => Set<ScheduledJob>().AsNoTracking();
    public IQueryable<JobSchedule> JobSchedules => Set<JobSchedule>().AsNoTracking();
    public IQueryable<JobDependency> JobDependencies => Set<JobDependency>().AsNoTracking();
    public IQueryable<JobExecution> JobExecutions => Set<JobExecution>().AsNoTracking();
    public IQueryable<JobPermissionEntry> JobPermissionEntries => Set<JobPermissionEntry>().AsNoTracking();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Default);
        modelBuilder.ApplyModuleConfigurations("Infrastructure.Database.Configurations.Scheduler");
    }
}
