using Domain.Files;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Files;

internal sealed class DeletedFileCleanupWorker(
    IServiceScopeFactory scopeFactory,
    FileCleanupOptions options,
    ILogger<DeletedFileCleanupWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Enabled)
        {
            return;
        }

        var interval = TimeSpan.FromSeconds(Math.Max(5, options.IntervalSeconds));
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Deleted file cleanup worker failed.");
            }

            await Task.Delay(interval, stoppingToken);
        }
    }

    private async Task CleanupBatchAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = scopeFactory.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        IFileObjectStorage storage = scope.ServiceProvider.GetRequiredService<IFileObjectStorage>();

        List<FileAsset> candidates = await dbContext.FileAssets
            .Where(x => x.IsDeleted && x.DeletedAtUtc != null && x.StorageDeletedAtUtc == null)
            .OrderBy(x => x.DeletedAtUtc)
            .Take(Math.Clamp(options.BatchSize, 1, 500))
            .ToListAsync(cancellationToken);

        if (candidates.Count == 0)
        {
            return;
        }

        foreach (FileAsset file in candidates)
        {
            try
            {
                await storage.DeleteAsync(file.ObjectKey, cancellationToken);
                file.StorageDeletedAtUtc = DateTime.UtcNow;
            }
            catch (FileNotFoundException)
            {
                // Object is already missing; mark as cleaned up to prevent endless retries.
                file.StorageDeletedAtUtc = DateTime.UtcNow;
            }
            catch (Exception exception)
            {
                logger.LogWarning(
                    exception,
                    "Failed to cleanup deleted file object. FileId={FileId} ObjectKey={ObjectKey}",
                    file.Id,
                    file.ObjectKey);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
