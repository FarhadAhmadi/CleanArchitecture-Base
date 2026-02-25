using Domain.Files;
using Application.Modules.Files;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Files;

internal sealed class FileUploadProcessingWorker(
    IServiceScopeFactory scopeFactory,
    FileUploadProcessingOptions options,
    ILogger<FileUploadProcessingWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!options.Enabled)
        {
            logger.LogInformation("File upload processing worker is disabled.");
            return;
        }

        var interval = TimeSpan.FromSeconds(Math.Max(1, options.PollingSeconds));
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "File upload processing worker failed.");
            }

            await Task.Delay(interval, stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = scopeFactory.CreateScope();
        FilesWriteDbContext dbContext = scope.ServiceProvider.GetRequiredService<FilesWriteDbContext>();
        IFileObjectStorage storage = scope.ServiceProvider.GetRequiredService<IFileObjectStorage>();

        List<FileAsset> pending = await dbContext.FileAssets
            .Where(x => !x.IsDeleted && x.StorageStatus == FileStorageStatus.Pending)
            .OrderBy(x => x.UploadRequestedAtUtc)
            .Take(Math.Clamp(options.BatchSize, 1, 200))
            .ToListAsync(cancellationToken);

        if (pending.Count == 0)
        {
            return;
        }

        DateTime now = DateTime.UtcNow;

        foreach (FileAsset file in pending)
        {
            try
            {
                string stagedKey = string.IsNullOrWhiteSpace(file.StagingObjectKey) ? file.ObjectKey : file.StagingObjectKey;
                (Stream Content, string ContentType) payload = await storage.OpenReadAsync(stagedKey, cancellationToken);
                await using (Stream contentStream = payload.Content)
                {
                    await storage.UploadAsync(
                        file.ObjectKey,
                        contentStream,
                        file.SizeBytes,
                        string.IsNullOrWhiteSpace(file.ContentType) ? payload.ContentType : file.ContentType,
                        cancellationToken);
                }

                if (!string.IsNullOrWhiteSpace(file.StagingObjectKey) &&
                    !string.Equals(file.StagingObjectKey, file.ObjectKey, StringComparison.Ordinal))
                {
                    await storage.DeleteAsync(file.StagingObjectKey, cancellationToken);
                }

                file.StorageStatus = FileStorageStatus.Available;
                file.StorageAvailableAtUtc = now;
                file.StorageError = null;
                file.StorageLastCheckedAtUtc = now;
                file.StagingObjectKey = null;
                file.StorageRetryCount = 0;
                file.UpdatedAtUtc = now;
                file.Raise(new FileUploadedDomainEvent(file.Id, file.OwnerUserId, file.Module, file.SizeBytes));
            }
            catch (FileNotFoundException exception)
            {
                file.StorageRetryCount += 1;
                file.StorageLastCheckedAtUtc = now;
                file.StorageError = exception.Message;
                file.StorageStatus = file.StorageRetryCount >= options.MaxRetryCount
                    ? FileStorageStatus.Failed
                    : FileStorageStatus.Missing;
                file.UpdatedAtUtc = now;
            }
            catch (Exception exception)
            {
                file.StorageRetryCount += 1;
                file.StorageLastCheckedAtUtc = now;
                file.StorageError = exception.Message;
                if (file.StorageRetryCount >= options.MaxRetryCount)
                {
                    file.StorageStatus = FileStorageStatus.Failed;
                }

                file.UpdatedAtUtc = now;
                logger.LogWarning(
                    exception,
                    "Failed to materialize pending file upload. FileId={FileId} RetryCount={RetryCount}",
                    file.Id,
                    file.StorageRetryCount);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
