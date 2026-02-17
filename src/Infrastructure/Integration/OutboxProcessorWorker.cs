using Infrastructure.Database;
using Infrastructure.DomainEvents;
using Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using SharedKernel;

namespace Infrastructure.Integration;

internal sealed class OutboxProcessorWorker(
    IServiceScopeFactory scopeFactory,
    OutboxOptions options,
    ILogger<OutboxProcessorWorker> logger) : BackgroundService
{
    private readonly ResiliencePipeline _retryPipeline = new ResiliencePipelineBuilder()
        .AddRetry(new RetryStrategyOptions
        {
            ShouldHandle = new PredicateBuilder().Handle<Exception>(),
            BackoffType = DelayBackoffType.Exponential,
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromMilliseconds(300)
        })
        .Build();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using IServiceScope scope = scopeFactory.CreateScope();
            ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            IDomainEventsDispatcher dispatcher = scope.ServiceProvider.GetRequiredService<IDomainEventsDispatcher>();
            IIntegrationEventSerializer serializer = scope.ServiceProvider.GetRequiredService<IIntegrationEventSerializer>();
            IIntegrationEventPublisher publisher = scope.ServiceProvider.GetRequiredService<IIntegrationEventPublisher>();

            List<OutboxMessage> messages = await dbContext.Set<OutboxMessage>()
                .Where(x => x.ProcessedOnUtc == null && x.RetryCount < options.MaxRetryCount)
                .OrderBy(x => x.OccurredOnUtc)
                .Take(options.BatchSize)
                .ToListAsync(stoppingToken);

            if (messages.Count == 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(options.PollingIntervalSeconds), stoppingToken);
                continue;
            }

            foreach (OutboxMessage message in messages)
            {
                await ProcessMessageAsync(message, serializer, dispatcher, publisher, dbContext, stoppingToken);
            }

            await dbContext.SaveChangesAsync(stoppingToken);
        }
    }

    private async Task ProcessMessageAsync(
        OutboxMessage message,
        IIntegrationEventSerializer serializer,
        IDomainEventsDispatcher dispatcher,
        IIntegrationEventPublisher publisher,
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        try
        {
            await _retryPipeline.ExecuteAsync(async token =>
            {
                if (serializer.Deserialize(message.Type, message.Payload) is not IDomainEvent domainEvent)
                {
                    throw new InvalidOperationException($"Unknown event type '{message.Type}'.");
                }

                await publisher.PublishAsync(message.Id, message.Type, message.Payload, token);
                await dispatcher.DispatchAsync([domainEvent], token);
            }, cancellationToken);

            message.ProcessedOnUtc = DateTime.UtcNow;
            message.Error = null;
        }
        catch (Exception ex)
        {
            message.RetryCount++;
            message.Error = ex.Message.Length > 2000 ? ex.Message[..2000] : ex.Message;

            if (logger.IsEnabled(LogLevel.Error))
            {
                logger.LogError(
                    ex,
                    "Outbox message processing failed. MessageId={MessageId} RetryCount={RetryCount}",
                    message.Id,
                    message.RetryCount);
            }
        }

        dbContext.Set<OutboxMessage>().Update(message);
    }
}
