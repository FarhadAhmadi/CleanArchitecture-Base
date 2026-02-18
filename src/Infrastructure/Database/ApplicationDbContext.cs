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
using SharedKernel;

namespace Infrastructure.Database;

public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    IntegrationEventSerializer integrationEventSerializer)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<LogEvent> LogEvents { get; set; }
    public DbSet<AlertRule> AlertRules { get; set; }
    public DbSet<AlertIncident> AlertIncidents { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
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
    internal DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    internal DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        modelBuilder.HasDefaultSchema(Schemas.Default);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
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

    private List<IDomainEvent> CollectDomainEvents()
    {
        return [.. ChangeTracker
            .Entries<Entity>()
            .Select(entry => entry.Entity)
            .SelectMany(entity => entity.DomainEvents)];
    }

    private void ClearDomainEvents()
    {
        foreach (Entity entity in ChangeTracker.Entries<Entity>().Select(entry => entry.Entity))
        {
            entity.ClearDomainEvents();
        }
    }
}
