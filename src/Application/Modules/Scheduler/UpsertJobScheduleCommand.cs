using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Modules.Scheduler;
using Domain.Scheduler;
using Infrastructure.Auditing;
using Microsoft.EntityFrameworkCore;

namespace Application.Scheduler;

public sealed record UpsertJobScheduleCommand(
    Guid JobId,
    ScheduleType Type,
    string? CronExpression,
    int? IntervalSeconds,
    DateTime? OneTimeAtUtc,
    DateTime? StartAtUtc,
    DateTime? EndAtUtc,
    MisfirePolicy MisfirePolicy,
    int? MaxCatchUpRuns) : ICommand<IResult>;

internal sealed class UpsertJobScheduleCommandHandler(
    IApplicationDbContext dbContext,
    IUserContext userContext,
    IAuditTrailService auditTrailService) : ResultWrappingCommandHandler<UpsertJobScheduleCommand>
{
    protected override async Task<IResult> HandleCore(UpsertJobScheduleCommand command, CancellationToken cancellationToken)
    {
        ScheduledJob? job = await dbContext.ScheduledJobs
            .SingleOrDefaultAsync(x => x.Id == command.JobId, cancellationToken);

        if (job is null)
        {
            return Results.NotFound();
        }

        if (command.EndAtUtc.HasValue && command.StartAtUtc.HasValue && command.EndAtUtc < command.StartAtUtc)
        {
            return Results.BadRequest(new { message = "EndAtUtc must be greater than StartAtUtc." });
        }

        if (!SchedulerCalculations.TryValidateSchedule(
            command.Type,
            command.CronExpression,
            command.IntervalSeconds,
            command.OneTimeAtUtc,
            out string? validationError))
        {
            return Results.BadRequest(new { message = validationError });
        }

        JobSchedule? schedule = await dbContext.JobSchedules
            .SingleOrDefaultAsync(x => x.JobId == command.JobId, cancellationToken);

        DateTime nowUtc = DateTime.UtcNow;
        DateTime? nextRun = SchedulerCalculations.ComputeNextRunUtc(
            command.Type,
            command.CronExpression,
            command.IntervalSeconds,
            command.OneTimeAtUtc,
            nowUtc);

        if (schedule is null)
        {
            schedule = new JobSchedule
            {
                Id = Guid.NewGuid(),
                JobId = command.JobId,
                CreatedAtUtc = nowUtc
            };
            dbContext.JobSchedules.Add(schedule);
        }

        schedule.Type = command.Type;
        schedule.CronExpression = string.IsNullOrWhiteSpace(command.CronExpression) ? null : command.CronExpression.Trim();
        schedule.IntervalSeconds = command.IntervalSeconds;
        schedule.OneTimeAtUtc = command.OneTimeAtUtc;
        schedule.StartAtUtc = command.StartAtUtc;
        schedule.EndAtUtc = command.EndAtUtc;
        schedule.IsEnabled = true;
        schedule.MisfirePolicy = command.MisfirePolicy;
        schedule.MaxCatchUpRuns = Math.Clamp(command.MaxCatchUpRuns ?? 1, 1, 50);
        schedule.NextRunAtUtc = nextRun;
        schedule.RetryAttempt = 0;
        schedule.UpdatedAtUtc = nowUtc;

        await dbContext.SaveChangesAsync(cancellationToken);

        await auditTrailService.RecordAsync(
            new AuditRecordRequest(
                userContext.UserId.ToString("N"),
                "scheduler.schedule.upsert",
                "JobSchedule",
                schedule.Id.ToString("N"),
                $"{{\"jobId\":\"{command.JobId:N}\",\"type\":\"{command.Type}\"}}"),
            cancellationToken);

        return Results.Ok(new
        {
            schedule.Id,
            schedule.JobId,
            type = schedule.Type.ToString(),
            schedule.NextRunAtUtc,
            schedule.IsEnabled,
            misfirePolicy = schedule.MisfirePolicy.ToString(),
            schedule.MaxCatchUpRuns
        });
    }
}
