using Domain.Logging;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Logging;

internal sealed class LogRetryWorker(
    ILogIngestionQueue queue,
    IServiceScopeFactory scopeFactory,
    LoggingOptions options,
    ILogger<LogRetryWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            LogEvent item = await queue.DequeueAsync(stoppingToken);

            bool saved = false;
            int attempt = 0;

            while (!saved && attempt < options.RetryMaxAttempts && !stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using IServiceScope scope = scopeFactory.CreateScope();
                    ApplicationDbContext db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    bool duplicate = !string.IsNullOrWhiteSpace(item.IdempotencyKey) &&
                                     await db.LogEvents.AnyAsync(x => x.IdempotencyKey == item.IdempotencyKey, stoppingToken);

                    if (!duplicate)
                    {
                        db.LogEvents.Add(item);
                        await db.SaveChangesAsync(stoppingToken);
                    }

                    saved = true;
                }
                catch (Exception ex)
                {
                    attempt++;
                    int delayMs = options.RetryInitialDelayMs * (int)Math.Pow(2, Math.Min(attempt, 8));
                    logger.LogWarning(ex, "Retry write for queued log failed. Attempt {Attempt}", attempt);
                    await Task.Delay(delayMs, stoppingToken);
                }
            }
        }
    }
}
