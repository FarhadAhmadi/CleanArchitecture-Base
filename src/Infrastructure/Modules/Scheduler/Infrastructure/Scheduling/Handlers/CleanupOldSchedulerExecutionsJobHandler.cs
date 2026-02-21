using System.Text.Json;
using Application.Abstractions.Scheduler;
using Domain.Modules.Scheduler;
using Domain.Scheduler;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Scheduler;

internal sealed class CleanupOldSchedulerExecutionsJobHandler(
    ApplicationDbContext dbContext,
    ILogger<CleanupOldSchedulerExecutionsJobHandler> logger) : IScheduledJobHandler
{
    public JobType JobType => JobType.CleanupOldSchedulerExecutions;

    public async Task ExecuteAsync(ScheduledJob job, CancellationToken cancellationToken)
    {
        int retentionDays = 14;
        if (!string.IsNullOrWhiteSpace(job.PayloadJson))
        {
            try
            {
                CleanupPayload? payload = JsonSerializer.Deserialize<CleanupPayload>(job.PayloadJson);
                if (payload?.RetentionDays > 0)
                {
                    retentionDays = payload.RetentionDays.Value;
                }
            }
            catch (JsonException ex)
            {
                if (logger.IsEnabled(LogLevel.Warning))
                {
                    logger.LogWarning(ex, "Invalid payload for cleanup scheduler job. JobId={JobId}", job.Id);
                }
            }
        }

        DateTime threshold = DateTime.UtcNow.AddDays(-retentionDays);
        int deleted = await dbContext.JobExecutions
            .Where(x => x.FinishedAtUtc.HasValue && x.FinishedAtUtc.Value < threshold)
            .ExecuteDeleteAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "CleanupOldSchedulerExecutions completed. JobId={JobId} RetentionDays={RetentionDays} Deleted={Deleted}",
                job.Id,
                retentionDays,
                deleted);
        }
    }

    private sealed record CleanupPayload(int? RetentionDays);
}
