using Domain.Notifications;
using Infrastructure.Database;
using Infrastructure.Integration;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Monitoring;

public sealed class OrchestrationHealthService(ApplicationReadDbContext dbContext, OutboxOptions outboxOptions)
{
    public async Task<OrchestrationHealthSnapshot> GetSnapshotAsync(CancellationToken cancellationToken)
    {
        DateTime nowUtc = DateTime.UtcNow;

        int outboxPending = await dbContext.OutboxMessages
            .CountAsync(x => x.ProcessedOnUtc == null && x.RetryCount < outboxOptions.MaxRetryCount, cancellationToken);
        int outboxFailed = await dbContext.OutboxMessages
            .CountAsync(x => x.ProcessedOnUtc == null && x.RetryCount >= outboxOptions.MaxRetryCount, cancellationToken);

        int inboxPending = await dbContext.Set<InboxMessage>()
            .CountAsync(x => x.ProcessedOnUtc == null && x.Error == null, cancellationToken);
        int inboxFailed = await dbContext.Set<InboxMessage>()
            .CountAsync(x => x.ProcessedOnUtc == null && x.Error != null, cancellationToken);

        int alertsQueued = await dbContext.AlertIncidents
            .CountAsync(x => x.Status == "Queued" || x.Status == "Dispatching", cancellationToken);
        int alertsFailed = await dbContext.AlertIncidents
            .CountAsync(x => x.Status == "Failed", cancellationToken);

        int notificationsPending = await dbContext.NotificationMessages
            .CountAsync(
                x => x.Status == NotificationStatus.Pending || x.Status == NotificationStatus.Scheduled,
                cancellationToken);
        int notificationsFailed = await dbContext.NotificationMessages
            .CountAsync(x => x.Status == NotificationStatus.Failed, cancellationToken);

        DateTime? oldestOutboxPendingUtc = await dbContext.OutboxMessages
            .Where(x => x.ProcessedOnUtc == null)
            .OrderBy(x => x.OccurredOnUtc)
            .Select(x => (DateTime?)x.OccurredOnUtc)
            .FirstOrDefaultAsync(cancellationToken);

        DateTime? oldestInboxPendingUtc = await dbContext.Set<InboxMessage>()
            .Where(x => x.ProcessedOnUtc == null)
            .OrderBy(x => x.ReceivedOnUtc)
            .Select(x => (DateTime?)x.ReceivedOnUtc)
            .FirstOrDefaultAsync(cancellationToken);

        DateTime? oldestNotificationPendingUtc = await dbContext.NotificationMessages
            .Where(x => x.Status == NotificationStatus.Pending || x.Status == NotificationStatus.Scheduled)
            .OrderBy(x => x.CreatedAtUtc)
            .Select(x => (DateTime?)x.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        bool isHealthy = outboxFailed == 0 && inboxFailed == 0 && alertsFailed == 0 && notificationsFailed == 0;
        string status = isHealthy ? "Healthy" : "Degraded";

        return new OrchestrationHealthSnapshot(
            status,
            nowUtc,
            outboxPending,
            outboxFailed,
            inboxPending,
            inboxFailed,
            alertsQueued,
            alertsFailed,
            notificationsPending,
            notificationsFailed,
            oldestOutboxPendingUtc,
            oldestInboxPendingUtc,
            oldestNotificationPendingUtc);
    }
}

public sealed record OrchestrationHealthSnapshot(
    string Status,
    DateTime TimestampUtc,
    int OutboxPending,
    int OutboxFailed,
    int InboxPending,
    int InboxFailed,
    int AlertsQueued,
    int AlertsFailed,
    int NotificationsPending,
    int NotificationsFailed,
    DateTime? OldestOutboxPendingUtc,
    DateTime? OldestInboxPendingUtc,
    DateTime? OldestNotificationPendingUtc);
