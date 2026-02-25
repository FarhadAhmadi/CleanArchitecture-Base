using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Infrastructure;

internal sealed class IdempotencyCleanupWorker(
    IdempotencyOptions options,
    IServiceScopeFactory scopeFactory,
    ILogger<IdempotencyCleanupWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Enabled)
        {
            return;
        }

        var interval = TimeSpan.FromMinutes(Math.Max(1, options.CleanupIntervalMinutes));
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using IServiceScope scope = scopeFactory.CreateScope();
                ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                DateTime now = DateTime.UtcNow;
                int batchSize = Math.Clamp(options.CleanupBatchSize, 10, 5000);

                List<Guid> expiredIds = await dbContext.IdempotencyRequests
                    .Where(x => x.ExpiresAtUtc < now)
                    .OrderBy(x => x.ExpiresAtUtc)
                    .Select(x => x.Id)
                    .Take(batchSize)
                    .ToListAsync(stoppingToken);

                if (expiredIds.Count > 0)
                {
                    int deletedCount = await dbContext.IdempotencyRequests
                        .Where(x => expiredIds.Contains(x.Id))
                        .ExecuteDeleteAsync(stoppingToken);

                    if (logger.IsEnabled(LogLevel.Debug))
                    {
                        logger.LogDebug("Idempotency cleanup removed {DeletedCount} expired record(s).", deletedCount);
                    }
                }
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Idempotency cleanup worker failed.");
            }

            await Task.Delay(interval, stoppingToken);
        }
    }
}
