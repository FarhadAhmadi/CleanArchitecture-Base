namespace Application.Abstractions.Observability;

public interface IOrchestrationHealthService
{
    Task<OrchestrationHealthSnapshot> GetSnapshotAsync(CancellationToken cancellationToken);
}

public interface IOrchestrationReplayService
{
    Task<int> ReplayFailedOutboxAsync(int take, CancellationToken cancellationToken);
    Task<int> ReplayFailedInboxAsync(int take, CancellationToken cancellationToken);
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
