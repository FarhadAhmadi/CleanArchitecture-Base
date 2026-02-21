using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Modules.Scheduler;
using Domain.Scheduler;
using Infrastructure.Auditing;
using Microsoft.EntityFrameworkCore;

namespace Application.Scheduler;

public sealed record DisableJobCommand(Guid JobId) : ICommand<IResult>;

internal sealed class DisableJobCommandHandler(
    IApplicationDbContext dbContext,
    IUserContext userContext,
    IAuditTrailService auditTrailService) : ResultWrappingCommandHandler<DisableJobCommand>
{
    protected override async Task<IResult> HandleCore(DisableJobCommand command, CancellationToken cancellationToken)
    {
        Domain.Modules.Scheduler.ScheduledJob? job = await dbContext.ScheduledJobs
            .SingleOrDefaultAsync(x => x.Id == command.JobId, cancellationToken);

        if (job is null)
        {
            return Results.NotFound();
        }

        job.Status = JobStatus.Inactive;
        job.Raise(new SchedulerJobStatusChangedDomainEvent(job.Id, job.Status, "disabled"));
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditTrailService.RecordAsync(
            new AuditRecordRequest(
                userContext.UserId.ToString("N"),
                "scheduler.job.disable",
                "ScheduledJob",
                job.Id.ToString("N"),
                "{}"),
            cancellationToken);

        return Results.Ok(new { job.Id, status = job.Status.ToString() });
    }
}
