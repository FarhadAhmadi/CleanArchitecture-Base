using Application.Abstractions.Data;
using Application.Abstractions.Authentication;
using Domain.Auditing;
using Domain.Authorization;
using Domain.Files;
using Domain.Logging;
using Domain.Notifications;
using Domain.Profiles;
using Domain.Todos;
using Domain.Users;
using Infrastructure.Integration;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SharedKernel;
using Domain.Modules.Notifications;

namespace Infrastructure.Database;

public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    IntegrationEventSerializer integrationEventSerializer,
    IUserContext? userContext = null)
    : IdentityDbContext<
        User,
        Role,
        Guid,
        IdentityUserClaim<Guid>,
        UserRole,
        IdentityUserLogin<Guid>,
        IdentityRoleClaim<Guid>,
        IdentityUserToken<Guid>>(options), IApplicationDbContext
{
    public DbSet<LogEvent> LogEvents { get; set; }
    public DbSet<AlertRule> AlertRules { get; set; }
    public DbSet<AlertIncident> AlertIncidents { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<UserPermission> UserPermissions { get; set; }
    public DbSet<TodoItem> TodoItems { get; set; }
    public DbSet<UserExternalLogin> UserExternalLogins { get; set; }
    public DbSet<AuditEntry> AuditEntries { get; set; }
    public DbSet<FileAsset> FileAssets { get; set; }
    public DbSet<FileTag> FileTags { get; set; }
    public DbSet<FileAccessAudit> FileAccessAudits { get; set; }
    public DbSet<FilePermissionEntry> FilePermissionEntries { get; set; }
    public DbSet<NotificationMessage> NotificationMessages { get; set; }
    public DbSet<NotificationTemplate> NotificationTemplates { get; set; }
    public DbSet<NotificationTemplateRevision> NotificationTemplateRevisions { get; set; }
    public DbSet<NotificationSchedule> NotificationSchedules { get; set; }
    public DbSet<NotificationPermissionEntry> NotificationPermissionEntries { get; set; }
    public DbSet<NotificationDeliveryAttempt> NotificationDeliveryAttempts { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    internal DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    internal DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();
    DbSet<User> IApplicationDbContext.Users => base.Users;
    DbSet<Role> IApplicationDbContext.Roles => base.Roles;
    DbSet<UserRole> IApplicationDbContext.UserRoles => base.UserRoles;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims", Schemas.Auth);
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins", Schemas.Auth);
        builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens", Schemas.Auth);
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims", Schemas.Auth);

        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        builder.HasDefaultSchema(Schemas.Default);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditFields();

        List<IDomainEvent> domainEvents = CollectDomainEvents();

        int result = await base.SaveChangesAsync(cancellationToken);

        if (domainEvents.Count != 0)
        {
            List<OutboxMessage> outboxMessages = [.. domainEvents.Select(integrationEventSerializer.ToOutboxMessage)];
            OutboxMessages.AddRange(outboxMessages);
            await base.SaveChangesAsync(cancellationToken);
            ClearDomainEvents();
        }

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
            return userContext?.UserId.ToString("N") ?? "system";
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
