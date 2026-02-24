using Domain.Files;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Files;

internal sealed class FileStorageReconciliationWorker(
    IServiceScopeFactory scopeFactory,
    FileReconciliationOptions options,
    ILogger<FileStorageReconciliationWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Enabled)
        {
            logger.LogInformation("File storage reconciliation worker is disabled.");
            return;
        }

        var interval = TimeSpan.FromSeconds(Math.Max(30, options.PollingSeconds));
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ReconcileAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "File storage reconciliation worker failed.");
            }

            await Task.Delay(interval, stoppingToken);
        }
    }

    private async Task ReconcileAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = scopeFactory.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        IFileObjectStorage storage = scope.ServiceProvider.GetRequiredService<IFileObjectStorage>();

        DateTime now = DateTime.UtcNow;
        DateTime staleThreshold = now.AddMinutes(-Math.Max(1, options.PendingStaleMinutes));

        List<FileAsset> pendingStale = await dbContext.FileAssets
            .Where(x => !x.IsDeleted &&
                        x.StorageStatus == FileStorageStatus.Pending &&
                        x.UploadRequestedAtUtc < staleThreshold)
            .OrderBy(x => x.UploadRequestedAtUtc)
            .Take(Math.Clamp(options.BatchSize, 1, 500))
            .ToListAsync(cancellationToken);

        foreach (FileAsset file in pendingStale)
        {
            file.StorageStatus = FileStorageStatus.Failed;
            file.StorageError = "Pending file exceeded reconciliation threshold.";
            file.StorageLastCheckedAtUtc = now;
            file.UpdatedAtUtc = now;
        }

        List<FileAsset> available = await dbContext.FileAssets
            .Where(x => !x.IsDeleted && x.StorageStatus == FileStorageStatus.Available)
            .OrderBy(x => x.StorageLastCheckedAtUtc ?? x.UploadedAtUtc)
            .Take(Math.Clamp(options.BatchSize, 1, 500))
            .ToListAsync(cancellationToken);

        foreach (FileAsset file in available)
        {
            bool exists = await storage.ExistsAsync(file.ObjectKey, cancellationToken);
            file.StorageLastCheckedAtUtc = now;
            if (exists)
            {
                continue;
            }

            file.StorageStatus = FileStorageStatus.Missing;
            file.StorageError = "Object does not exist in storage.";
            file.UpdatedAtUtc = now;
        }

        if (pendingStale.Count != 0 || available.Count != 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
