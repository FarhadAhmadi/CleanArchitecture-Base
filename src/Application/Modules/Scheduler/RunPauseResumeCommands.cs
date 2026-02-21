using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.Abstractions.Scheduler;
using Domain.Modules.Scheduler;
using Domain.Scheduler;
using Infrastructure.Auditing;
using Microsoft.EntityFrameworkCore;

namespace Application.Scheduler;

public sealed record RunJobNowCommand(Guid JobId) : ICommand<IResult>;
public sealed record PauseJobCommand(Guid JobId) : ICommand<IResult>;
public sealed record ResumeJobCommand(Guid JobId) : ICommand<IResult>;
public sealed record ReplayDeadLetteredRunsCommand(Guid JobId) : ICommand<IResult>;
public sealed record QuarantineJobCommand(Guid JobId, int? QuarantineMinutes, string? Reason) : ICommand<IResult>;

internal sealed class RunJobNowCommandHandler(
    IApplicationDbContext dbContext,
    IUserContext userContext,
    ISchedulerExecutionService schedulerExecutionService,
    IAuditTrailService auditTrailService) : ResultWrappingCommandHandler<RunJobNowCommand>
{
    protected override async Task<IResult> HandleCore(RunJobNowCommand command, CancellationToken cancellationToken)
    {
        ScheduledJob? job = await dbContext.ScheduledJobs
            .SingleOrDefaultAsync(x => x.Id == command.JobId, cancellationToken);

        if (job is null)
        {
            return Results.NotFound();
        }

        DateTime nowUtc = DateTime.UtcNow;
        SchedulerExecutionResult executionResult = await schedulerExecutionService.ExecuteAsync(
            new SchedulerExecutionRequest(
                job,
                userContext.UserId.ToString("N"),
                "manual",
                1,
                1,
                nowUtc,
                job.IsQuarantined || job.ConsecutiveFailures > 0),
            cancellationToken);

        var execution = new JobExecution
        {
            Id = Guid.NewGuid(),
            JobId = job.Id,
            Status = executionResult.Status,
            TriggeredBy = userContext.UserId.ToString("N"),
            NodeId = "manual",
            ScheduledAtUtc = nowUtc,
            StartedAtUtc = nowUtc,
            FinishedAtUtc = DateTime.UtcNow,
            DurationMs = executionResult.DurationMs,
            Attempt = 1,
            MaxAttempts = 1,
            IsReplay = job.IsQuarantined || job.ConsecutiveFailures > 0,
            Error = executionResult.Error
        };

        dbContext.JobExecutions.Add(execution);

        if (executionResult.Status == JobExecutionStatus.Succeeded)
        {
            job.ConsecutiveFailures = 0;
            job.IsQuarantined = false;
            job.QuarantinedUntilUtc = null;
            job.DeadLetterReason = null;
            if (job.Status == JobStatus.Quarantined)
            {
                job.Status = JobStatus.Active;
            }
        }
        else if (executionResult.Status is JobExecutionStatus.Failed or JobExecutionStatus.TimedOut or JobExecutionStatus.Canceled)
        {
            job.ConsecutiveFailures++;
            job.LastFailureAtUtc = DateTime.UtcNow;
        }

        job.LastRunAtUtc = nowUtc;
        job.LastExecutionStatus = execution.Status;

        JobSchedule? schedule = await dbContext.JobSchedules
            .SingleOrDefaultAsync(x => x.JobId == job.Id, cancellationToken);

        if (schedule is not null)
        {
            schedule.NextRunAtUtc = SchedulerCalculations.ComputeNextRunUtc(
                schedule.Type,
                schedule.CronExpression,
                schedule.IntervalSeconds,
                schedule.OneTimeAtUtc,
                nowUtc);

            if (schedule.Type == ScheduleType.OneTime)
            {
                schedule.IsEnabled = false;
            }

            schedule.RetryAttempt = 0;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        await auditTrailService.RecordAsync(
            new AuditRecordRequest(
                userContext.UserId.ToString("N"),
                "scheduler.job.run",
                "ScheduledJob",
                job.Id.ToString("N"),
                "{}"),
            cancellationToken);

        return Results.Ok(new { execution.Id, status = execution.Status.ToString(), execution.StartedAtUtc, execution.Error });
    }
}

internal sealed class PauseJobCommandHandler(
    IApplicationDbContext dbContext,
    IUserContext userContext,
    IAuditTrailService auditTrailService) : ResultWrappingCommandHandler<PauseJobCommand>
{
    protected override async Task<IResult> HandleCore(PauseJobCommand command, CancellationToken cancellationToken)
    {
        ScheduledJob? job = await dbContext.ScheduledJobs
            .SingleOrDefaultAsync(x => x.Id == command.JobId, cancellationToken);

        if (job is null)
        {
            return Results.NotFound();
        }

        job.Status = JobStatus.Paused;
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditTrailService.RecordAsync(
            new AuditRecordRequest(
                userContext.UserId.ToString("N"),
                "scheduler.job.pause",
                "ScheduledJob",
                job.Id.ToString("N"),
                "{}"),
            cancellationToken);

        return Results.Ok(new { job.Id, status = job.Status.ToString() });
    }
}

internal sealed class ResumeJobCommandHandler(
    IApplicationDbContext dbContext,
    IUserContext userContext,
    IAuditTrailService auditTrailService) : ResultWrappingCommandHandler<ResumeJobCommand>
{
    protected override async Task<IResult> HandleCore(ResumeJobCommand command, CancellationToken cancellationToken)
    {
        ScheduledJob? job = await dbContext.ScheduledJobs
            .SingleOrDefaultAsync(x => x.Id == command.JobId, cancellationToken);

        if (job is null)
        {
            return Results.NotFound();
        }

        job.Status = JobStatus.Active;
        job.IsQuarantined = false;
        job.QuarantinedUntilUtc = null;
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditTrailService.RecordAsync(
            new AuditRecordRequest(
                userContext.UserId.ToString("N"),
                "scheduler.job.resume",
                "ScheduledJob",
                job.Id.ToString("N"),
                "{}"),
            cancellationToken);

        return Results.Ok(new { job.Id, status = job.Status.ToString() });
    }
}

internal sealed class ReplayDeadLetteredRunsCommandHandler(
    IApplicationDbContext dbContext,
    IUserContext userContext,
    IAuditTrailService auditTrailService) : ResultWrappingCommandHandler<ReplayDeadLetteredRunsCommand>
{
    protected override async Task<IResult> HandleCore(ReplayDeadLetteredRunsCommand command, CancellationToken cancellationToken)
    {
        ScheduledJob? job = await dbContext.ScheduledJobs
            .SingleOrDefaultAsync(x => x.Id == command.JobId, cancellationToken);

        if (job is null)
        {
            return Results.NotFound();
        }

        JobSchedule? schedule = await dbContext.JobSchedules
            .SingleOrDefaultAsync(x => x.JobId == command.JobId, cancellationToken);

        job.Status = JobStatus.Active;
        job.IsQuarantined = false;
        job.QuarantinedUntilUtc = null;
        job.ConsecutiveFailures = 0;
        job.DeadLetterReason = null;

        if (schedule is not null)
        {
            schedule.IsEnabled = true;
            schedule.RetryAttempt = 0;
            schedule.NextRunAtUtc = DateTime.UtcNow;
            schedule.UpdatedAtUtc = DateTime.UtcNow;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        await auditTrailService.RecordAsync(
            new AuditRecordRequest(
                userContext.UserId.ToString("N"),
                "scheduler.job.replay",
                "ScheduledJob",
                job.Id.ToString("N"),
                "{}"),
            cancellationToken);

        return Results.Ok(new { job.Id, status = job.Status.ToString(), replayScheduledAtUtc = DateTime.UtcNow });
    }
}

internal sealed class QuarantineJobCommandHandler(
    IApplicationDbContext dbContext,
    IUserContext userContext,
    IAuditTrailService auditTrailService) : ResultWrappingCommandHandler<QuarantineJobCommand>
{
    protected override async Task<IResult> HandleCore(QuarantineJobCommand command, CancellationToken cancellationToken)
    {
        ScheduledJob? job = await dbContext.ScheduledJobs
            .SingleOrDefaultAsync(x => x.Id == command.JobId, cancellationToken);

        if (job is null)
        {
            return Results.NotFound();
        }

        int minutes = Math.Clamp(command.QuarantineMinutes ?? 60, 1, 7 * 24 * 60);
        job.Status = JobStatus.Quarantined;
        job.IsQuarantined = true;
        job.QuarantinedUntilUtc = DateTime.UtcNow.AddMinutes(minutes);
        job.DeadLetterReason = string.IsNullOrWhiteSpace(command.Reason) ? "Manual quarantine" : command.Reason.Trim();

        JobSchedule? schedule = await dbContext.JobSchedules
            .SingleOrDefaultAsync(x => x.JobId == command.JobId, cancellationToken);

        DisableSchedule(schedule);

        await dbContext.SaveChangesAsync(cancellationToken);

        await auditTrailService.RecordAsync(
            new AuditRecordRequest(
                userContext.UserId.ToString("N"),
                "scheduler.job.quarantine",
                "ScheduledJob",
                job.Id.ToString("N"),
                $"{{\"minutes\":{minutes}}}"),
            cancellationToken);

        return Results.Ok(new { job.Id, status = job.Status.ToString(), job.QuarantinedUntilUtc, job.DeadLetterReason });
    }

    private static void DisableSchedule(JobSchedule? schedule)
    {
        if (schedule is null)
        {
            return;
        }

        schedule.IsEnabled = false;
    }
}
