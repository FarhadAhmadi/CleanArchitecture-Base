using Application.Abstractions.Scheduler;
using Application.Scheduler;
using Domain.Modules.Scheduler;
using Domain.Scheduler;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Scheduler;

internal sealed class SchedulerWorker(
    IServiceScopeFactory scopeFactory,
    SchedulerOptions options,
    ILogger<SchedulerWorker> logger) : BackgroundService
{
    private readonly string _nodeId = string.IsNullOrWhiteSpace(options.NodeId)
        ? $"{Environment.MachineName}:{Environment.ProcessId}"
        : options.NodeId.Trim();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Scheduler worker started. NodeId={NodeId}", _nodeId);
        }
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessDueJobsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Scheduler worker iteration failed.");
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(Math.Max(1, options.PollingIntervalSeconds)), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Scheduler worker stopped. NodeId={NodeId}", _nodeId);
        }
    }

    private async Task ProcessDueJobsAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = scopeFactory.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        ISchedulerExecutionService executionService = scope.ServiceProvider.GetRequiredService<ISchedulerExecutionService>();
        ISchedulerDistributedLockProvider lockProvider = scope.ServiceProvider.GetRequiredService<ISchedulerDistributedLockProvider>();
        ISchedulerRetryPolicyProvider retryPolicyProvider = scope.ServiceProvider.GetRequiredService<ISchedulerRetryPolicyProvider>();
        SchedulerMetrics metrics = scope.ServiceProvider.GetRequiredService<SchedulerMetrics>();

        string iterationLockName = "scheduler:iteration";
        bool hasIterationLock = await lockProvider.TryAcquireAsync(
            iterationLockName,
            TimeSpan.FromSeconds(Math.Max(5, options.LockLeaseSeconds)),
            cancellationToken);

        if (!hasIterationLock)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("Scheduler iteration skipped due to lock contention. NodeId={NodeId}", _nodeId);
            }
            return;
        }

        try
        {
            DateTime nowUtc = DateTime.UtcNow;

            List<Guid> dueJobIds = await dbContext.ScheduledJobs
                .Join(
                    dbContext.JobSchedules,
                    job => job.Id,
                    schedule => schedule.JobId,
                    (job, schedule) => new { Job = job, Schedule = schedule })
                .Where(x =>
                    x.Job.Status == JobStatus.Active &&
                    x.Schedule.IsEnabled &&
                    (!x.Schedule.StartAtUtc.HasValue || x.Schedule.StartAtUtc <= nowUtc) &&
                    (!x.Schedule.EndAtUtc.HasValue || x.Schedule.EndAtUtc >= nowUtc) &&
                    x.Schedule.NextRunAtUtc.HasValue &&
                    x.Schedule.NextRunAtUtc <= nowUtc)
                .OrderBy(x => x.Schedule.NextRunAtUtc)
                .Take(Math.Max(1, options.MaxDueJobsPerIteration))
                .Select(x => x.Job.Id)
                .ToListAsync(cancellationToken);

            if (dueJobIds.Count == 0)
            {
                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug("No due scheduler jobs found. NodeId={NodeId}", _nodeId);
                }
                return;
            }

            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Scheduler found due jobs. Count={Count} NodeId={NodeId}", dueJobIds.Count, _nodeId);
            }

            foreach (Guid jobId in dueJobIds)
            {
                await ProcessSingleJobAsync(
                    dbContext,
                    executionService,
                    lockProvider,
                    retryPolicyProvider,
                    metrics,
                    jobId,
                    cancellationToken);
            }
        }
        finally
        {
            await lockProvider.ReleaseAsync(iterationLockName, cancellationToken);
        }
    }

    private async Task ProcessSingleJobAsync(
        ApplicationDbContext dbContext,
        ISchedulerExecutionService executionService,
        ISchedulerDistributedLockProvider lockProvider,
        ISchedulerRetryPolicyProvider retryPolicyProvider,
        SchedulerMetrics metrics,
        Guid jobId,
        CancellationToken cancellationToken)
    {
        string jobLockName = $"scheduler:job:{jobId:N}";
        bool hasJobLock = await lockProvider.TryAcquireAsync(
            jobLockName,
            TimeSpan.FromSeconds(Math.Max(5, options.LockLeaseSeconds)),
            cancellationToken);

        if (!hasJobLock)
        {
            if (logger.IsEnabled(LogLevel.Debug))
            {
                logger.LogDebug("Skipped job due to lock contention. JobId={JobId} NodeId={NodeId}", jobId, _nodeId);
            }
            return;
        }

        try
        {
            ScheduledJob? job = await dbContext.ScheduledJobs.SingleOrDefaultAsync(x => x.Id == jobId, cancellationToken);
            JobSchedule? schedule = await dbContext.JobSchedules.SingleOrDefaultAsync(x => x.JobId == jobId, cancellationToken);
            if (job is null || schedule is null)
            {
                return;
            }

            DateTime nowUtc = DateTime.UtcNow;
            if (job.Status == JobStatus.Quarantined && job.QuarantinedUntilUtc.HasValue && job.QuarantinedUntilUtc <= nowUtc)
            {
                job.Status = JobStatus.Active;
                job.IsQuarantined = false;
                schedule.IsEnabled = true;
                schedule.RetryAttempt = 0;
            }

            if (!SchedulerCalculations.IsJobRunnable(job, schedule, nowUtc))
            {
                return;
            }

            if (!await DependenciesSatisfiedAsync(dbContext, job.Id, cancellationToken))
            {
                if (logger.IsEnabled(LogLevel.Debug))
                {
                    logger.LogDebug("Dependencies not satisfied. JobId={JobId}", job.Id);
                }
                return;
            }

            if (!schedule.NextRunAtUtc.HasValue)
            {
                schedule.NextRunAtUtc = SchedulerCalculations.ComputeNextRunUtc(
                    schedule.Type,
                    schedule.CronExpression,
                    schedule.IntervalSeconds,
                    schedule.OneTimeAtUtc,
                    nowUtc);
                await dbContext.SaveChangesAsync(cancellationToken);
                return;
            }

            List<DateTime> dueRunTimes = BuildDueRunTimes(schedule, nowUtc);
            if (dueRunTimes.Count == 0)
            {
                await SaveMisfireSkipExecutionAsync(dbContext, job, schedule, nowUtc, cancellationToken);
                return;
            }

            foreach (DateTime scheduledAtUtc in dueRunTimes)
            {
                SchedulerRetryPolicy retryPolicy = retryPolicyProvider.GetPolicy(job.Type, job);
                int attempt = schedule.RetryAttempt + 1;

                SchedulerExecutionResult result = await executionService.ExecuteAsync(
                    new SchedulerExecutionRequest(
                        job,
                        "scheduler-worker",
                        _nodeId,
                        attempt,
                        retryPolicy.MaxAttempts,
                        scheduledAtUtc,
                        false),
                    cancellationToken);

                DateTime finishedAtUtc = DateTime.UtcNow;
                JobExecutionStatus finalStatus = result.Status;
                bool canRetry = result.Status is JobExecutionStatus.Failed or JobExecutionStatus.TimedOut or JobExecutionStatus.Canceled;
                bool shouldRetry = canRetry && attempt < retryPolicy.MaxAttempts;

                var execution = new JobExecution
                {
                    Id = Guid.NewGuid(),
                    JobId = job.Id,
                    Status = finalStatus,
                    TriggeredBy = "scheduler-worker",
                    NodeId = _nodeId,
                    ScheduledAtUtc = scheduledAtUtc,
                    StartedAtUtc = scheduledAtUtc > nowUtc ? nowUtc : scheduledAtUtc,
                    FinishedAtUtc = finishedAtUtc,
                    DurationMs = result.DurationMs,
                    Attempt = attempt,
                    MaxAttempts = retryPolicy.MaxAttempts,
                    Error = result.Error,
                    PayloadSnapshotJson = job.PayloadJson
                };

                if (finalStatus == JobExecutionStatus.Succeeded)
                {
                    job.ConsecutiveFailures = 0;
                    job.LastFailureAtUtc = null;
                    job.DeadLetterReason = null;
                    schedule.RetryAttempt = 0;
                    schedule.NextRunAtUtc = SchedulerCalculations.ComputeNextRunUtc(
                        schedule.Type,
                        schedule.CronExpression,
                        schedule.IntervalSeconds,
                        schedule.OneTimeAtUtc,
                        scheduledAtUtc);

                    if (schedule.Type == ScheduleType.OneTime)
                    {
                        schedule.IsEnabled = false;
                    }
                }
                else if (shouldRetry)
                {
                    int delay = SchedulerCalculations.ComputeBackoffSeconds(
                        attempt,
                        retryPolicy.BaseDelaySeconds,
                        retryPolicy.MaxDelaySeconds);

                    schedule.RetryAttempt = attempt;
                    schedule.NextRunAtUtc = DateTime.UtcNow.AddSeconds(delay);
                    logger.LogWarning(
                        "Scheduler retry scheduled. JobId={JobId} Attempt={Attempt}/{MaxAttempts} DelaySeconds={DelaySeconds} Status={Status}",
                        job.Id,
                        attempt,
                        retryPolicy.MaxAttempts,
                        delay,
                        finalStatus);
                }
                else if (canRetry)
                {
                    schedule.RetryAttempt = 0;
                    job.ConsecutiveFailures++;
                    job.LastFailureAtUtc = DateTime.UtcNow;
                    schedule.NextRunAtUtc = SchedulerCalculations.ComputeNextRunUtc(
                        schedule.Type,
                        schedule.CronExpression,
                        schedule.IntervalSeconds,
                        schedule.OneTimeAtUtc,
                        scheduledAtUtc);

                    if (schedule.Type == ScheduleType.OneTime)
                    {
                        schedule.IsEnabled = false;
                    }

                    if (job.ConsecutiveFailures >= Math.Max(1, job.MaxConsecutiveFailures))
                    {
                        job.Status = JobStatus.Quarantined;
                        job.IsQuarantined = true;
                        job.QuarantinedUntilUtc = DateTime.UtcNow.AddMinutes(Math.Max(1, options.DefaultQuarantineMinutes));
                        schedule.IsEnabled = false;
                        execution.IsDeadLetter = true;
                        execution.DeadLetterReason = $"Exceeded max consecutive failures ({job.MaxConsecutiveFailures}).";
                        execution.Status = JobExecutionStatus.DeadLettered;
                        job.DeadLetterReason = execution.DeadLetterReason;

                        logger.LogError(
                            "Job moved to quarantine after repeated failures. JobId={JobId} ConsecutiveFailures={ConsecutiveFailures} QuarantinedUntilUtc={QuarantinedUntilUtc}",
                            job.Id,
                            job.ConsecutiveFailures,
                            job.QuarantinedUntilUtc);
                    }
                }

                dbContext.JobExecutions.Add(execution);
                job.LastRunAtUtc = DateTime.UtcNow;
                job.LastExecutionStatus = execution.Status;
                schedule.UpdatedAtUtc = DateTime.UtcNow;
                await dbContext.SaveChangesAsync(cancellationToken);

                double queueLagMs = Math.Max(0, (DateTime.UtcNow - scheduledAtUtc).TotalMilliseconds);
                metrics.RecordExecution(job.Type.ToString(), execution.Status, execution.DurationMs, queueLagMs);

                if (logger.IsEnabled(LogLevel.Information))
                {
                    logger.LogInformation(
                        "Scheduler execution completed. JobId={JobId} Status={Status} Attempt={Attempt}/{MaxAttempts} QueueLagMs={QueueLagMs} DurationMs={DurationMs}",
                        job.Id,
                        execution.Status,
                        execution.Attempt,
                        execution.MaxAttempts,
                        queueLagMs,
                        execution.DurationMs);
                }

                if (execution.Status is JobExecutionStatus.Failed or JobExecutionStatus.TimedOut or JobExecutionStatus.Canceled or JobExecutionStatus.DeadLettered)
                {
                    break;
                }
            }
        }
        finally
        {
            await lockProvider.ReleaseAsync(jobLockName, cancellationToken);
        }
    }

    private static async Task<bool> DependenciesSatisfiedAsync(
        ApplicationDbContext dbContext,
        Guid jobId,
        CancellationToken cancellationToken)
    {
        List<Guid> dependencyIds = await dbContext.JobDependencies
            .Where(x => x.JobId == jobId)
            .Select(x => x.DependsOnJobId)
            .ToListAsync(cancellationToken);

        if (dependencyIds.Count == 0)
        {
            return true;
        }

        foreach (Guid dependencyId in dependencyIds)
        {
            JobExecution? latest = await dbContext.JobExecutions
                .Where(x => x.JobId == dependencyId)
                .OrderByDescending(x => x.StartedAtUtc)
                .FirstOrDefaultAsync(cancellationToken);

            if (latest is null || latest.Status != JobExecutionStatus.Succeeded)
            {
                return false;
            }
        }

        return true;
    }

    private List<DateTime> BuildDueRunTimes(JobSchedule schedule, DateTime nowUtc)
    {
        DateTime? nextRunAtUtc = schedule.NextRunAtUtc;
        if (!nextRunAtUtc.HasValue || nextRunAtUtc > nowUtc)
        {
            return [];
        }

        DateTime misfireThreshold = nowUtc.AddSeconds(-Math.Max(1, options.MisfireGraceSeconds));
        if (nextRunAtUtc.Value < misfireThreshold && schedule.MisfirePolicy == MisfirePolicy.Skip)
        {
            schedule.LastMisfireAtUtc = nowUtc;
            schedule.NextRunAtUtc = AdvanceToFuture(schedule, nowUtc);
            return [];
        }

        if (schedule.MisfirePolicy != MisfirePolicy.CatchUp || schedule.Type == ScheduleType.OneTime)
        {
            return [nextRunAtUtc.Value];
        }

        int limit = Math.Max(1, schedule.MaxCatchUpRuns);
        var runs = new List<DateTime>(limit);
        DateTime cursor = nextRunAtUtc.Value;
        while (cursor <= nowUtc && runs.Count < limit)
        {
            runs.Add(cursor);
            DateTime? nextCursor = SchedulerCalculations.ComputeNextRunUtc(
                schedule.Type,
                schedule.CronExpression,
                schedule.IntervalSeconds,
                schedule.OneTimeAtUtc,
                cursor);
            if (!nextCursor.HasValue || nextCursor.Value <= cursor)
            {
                break;
            }

            cursor = nextCursor.Value;
        }

        return runs;
    }

    private static DateTime? AdvanceToFuture(JobSchedule schedule, DateTime nowUtc)
    {
        DateTime? cursor = schedule.NextRunAtUtc;
        int guard = 0;
        while (cursor.HasValue && cursor <= nowUtc && guard++ < 500)
        {
            cursor = SchedulerCalculations.ComputeNextRunUtc(
                schedule.Type,
                schedule.CronExpression,
                schedule.IntervalSeconds,
                schedule.OneTimeAtUtc,
                cursor.Value);
        }

        return cursor;
    }

    private async Task SaveMisfireSkipExecutionAsync(
        ApplicationDbContext dbContext,
        ScheduledJob job,
        JobSchedule schedule,
        DateTime nowUtc,
        CancellationToken cancellationToken)
    {
        var execution = new JobExecution
        {
            Id = Guid.NewGuid(),
            JobId = job.Id,
            Status = JobExecutionStatus.Skipped,
            TriggeredBy = "scheduler-worker",
            NodeId = _nodeId,
            ScheduledAtUtc = schedule.NextRunAtUtc ?? nowUtc,
            StartedAtUtc = nowUtc,
            FinishedAtUtc = nowUtc,
            DurationMs = 0,
            Attempt = 0,
            MaxAttempts = 0,
            Error = "Execution skipped by misfire policy."
        };

        dbContext.JobExecutions.Add(execution);
        job.LastExecutionStatus = JobExecutionStatus.Skipped;
        schedule.UpdatedAtUtc = nowUtc;
        await dbContext.SaveChangesAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Warning))
        {
            logger.LogWarning(
                "Scheduler skipped misfired run. JobId={JobId} Policy={Policy} NextRunAtUtc={NextRunAtUtc}",
                job.Id,
                schedule.MisfirePolicy,
                schedule.NextRunAtUtc);
        }
    }
}
