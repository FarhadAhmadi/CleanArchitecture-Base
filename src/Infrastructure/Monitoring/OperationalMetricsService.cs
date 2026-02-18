using Infrastructure.Database;
using Infrastructure.Integration;
using Infrastructure.Logging;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Monitoring;

public sealed class OperationalMetricsService(
    ApplicationReadDbContext dbContext,
    ILogIngestionQueue ingestionQueue,
    IAlertDispatchQueue alertQueue,
    OutboxOptions outboxOptions)
{
    public async Task<OperationalMetricsSnapshot> GetSnapshotAsync(CancellationToken cancellationToken)
    {
        IQueryable<Domain.Logging.LogEvent> logs = dbContext.LogEvents.AsNoTracking().Where(x => !x.IsDeleted);

        int totalLogEvents = await logs.CountAsync(cancellationToken);
        int corrupted = await logs.CountAsync(x => x.HasIntegrityIssue, cancellationToken);

        IQueryable<OutboxMessage> outbox = dbContext.OutboxMessages;
        int pending = await outbox.CountAsync(x => x.ProcessedOnUtc == null && x.RetryCount < outboxOptions.MaxRetryCount, cancellationToken);
        int failed = await outbox.CountAsync(x => x.ProcessedOnUtc == null && x.RetryCount >= outboxOptions.MaxRetryCount, cancellationToken);

        return new OperationalMetricsSnapshot(
            IngestionQueueDepth: ingestionQueue.ApproximateCount,
            IngestionDropped: ingestionQueue.DroppedCount,
            AlertQueueDepth: alertQueue.ApproximateCount,
            TotalLogEvents: totalLogEvents,
            CorruptedLogEvents: corrupted,
            OutboxPending: pending,
            OutboxFailed: failed,
            TimestampUtc: DateTime.UtcNow);
    }
}
