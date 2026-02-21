using Infrastructure.Database;
using Infrastructure.Integration;
using Infrastructure.DomainEvents;
using Application.Abstractions.Observability;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Infrastructure.Monitoring;

public sealed class OrchestrationReplayService(
    ApplicationDbContext dbContext,
    OutboxOptions outboxOptions,
    IntegrationEventSerializer serializer,
    IDomainEventsDispatcher dispatcher,
    ILogger<OrchestrationReplayService> logger) : IOrchestrationReplayService
{
    public async Task<int> ReplayFailedOutboxAsync(int take, CancellationToken cancellationToken)
    {
        int normalizedTake = Math.Clamp(take <= 0 ? 100 : take, 1, 2000);

        List<OutboxMessage> failed = await dbContext.Set<OutboxMessage>()
            .Where(x => x.ProcessedOnUtc == null && x.RetryCount >= outboxOptions.MaxRetryCount)
            .OrderBy(x => x.OccurredOnUtc)
            .Take(normalizedTake)
            .ToListAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Replay outbox requested. RequestedTake={RequestedTake} NormalizedTake={NormalizedTake} Selected={Selected}",
                take,
                normalizedTake,
                failed.Count);
        }

        foreach (OutboxMessage message in failed)
        {
            message.RetryCount = 0;
            message.Error = null;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return failed.Count;
    }

    public async Task<int> ReplayFailedInboxAsync(int take, CancellationToken cancellationToken)
    {
        int normalizedTake = Math.Clamp(take <= 0 ? 100 : take, 1, 2000);

        List<InboxMessage> failed = await dbContext.Set<InboxMessage>()
            .Where(x => x.ProcessedOnUtc == null && x.Error != null)
            .OrderBy(x => x.ReceivedOnUtc)
            .Take(normalizedTake)
            .ToListAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Replay inbox requested. RequestedTake={RequestedTake} NormalizedTake={NormalizedTake} Selected={Selected}",
                take,
                normalizedTake,
                failed.Count);
        }

        int replayed = 0;

        foreach (InboxMessage message in failed)
        {
            try
            {
                if (serializer.Deserialize(message.Type, message.Payload ?? string.Empty) is not IDomainEvent domainEvent)
                {
                    message.Error = "Replay failed: cannot deserialize payload.";
                    continue;
                }

                await dispatcher.DispatchAsync([domainEvent], cancellationToken);
                message.ProcessedOnUtc = DateTime.UtcNow;
                message.Error = null;
                replayed++;

                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug(
                        "Replay inbox message succeeded. MessageId={MessageId} Type={Type}",
                        message.Id,
                        message.Type);
                }
            }
            catch (Exception ex)
            {
                message.Error = ex.Message.Length > 2000 ? ex.Message[..2000] : ex.Message;

                if (logger.IsEnabled(LogLevel.Warning))
                {
                    logger.LogWarning(
                        ex,
                        "Replay inbox message failed. MessageId={MessageId} Type={Type}",
                        message.Id,
                        message.Type);
                }
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return replayed;
    }
}
