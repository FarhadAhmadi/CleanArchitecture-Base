using Domain.Logging;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace Infrastructure.Logging;

internal sealed class LogRetryWorker(
    ILogIngestionQueue queue,
    IServiceScopeFactory scopeFactory,
    ILogger<LogRetryWorker> logger) : BackgroundService
{
    private readonly ResiliencePipeline _retryPipeline = new ResiliencePipelineBuilder()
        .AddRetry(new RetryStrategyOptions
        {
            ShouldHandle = new PredicateBuilder().Handle<Exception>(),
            MaxRetryAttempts = 6,
            BackoffType = DelayBackoffType.Exponential,
            Delay = TimeSpan.FromMilliseconds(200)
        })
        .Build();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            LogEvent item = await queue.DequeueAsync(stoppingToken);

            try
            {
                await _retryPipeline.ExecuteAsync(async token =>
                {
                    using IServiceScope scope = scopeFactory.CreateScope();
                    ApplicationDbContext db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    bool duplicate = !string.IsNullOrWhiteSpace(item.IdempotencyKey) &&
                                     await db.LogEvents.AnyAsync(x => x.IdempotencyKey == item.IdempotencyKey, token);

                    if (!duplicate)
                    {
                        db.LogEvents.Add(item);
                        await db.SaveChangesAsync(token);
                    }
                }, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Retry write for queued log failed permanently. EventId={EventId}", item.Id);
            }
        }
    }
}
