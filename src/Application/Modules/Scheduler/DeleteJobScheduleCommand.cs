using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Infrastructure.Auditing;
using Microsoft.EntityFrameworkCore;

namespace Application.Scheduler;

public sealed record DeleteJobScheduleCommand(Guid JobId) : ICommand<IResult>;

internal sealed class DeleteJobScheduleCommandHandler(
    IApplicationDbContext dbContext,
    IUserContext userContext,
    IAuditTrailService auditTrailService) : ResultWrappingCommandHandler<DeleteJobScheduleCommand>
{
    protected override async Task<IResult> HandleCore(DeleteJobScheduleCommand command, CancellationToken cancellationToken)
    {
        Domain.Modules.Scheduler.JobSchedule? schedule = await dbContext.JobSchedules
            .SingleOrDefaultAsync(x => x.JobId == command.JobId, cancellationToken);

        if (schedule is null)
        {
            return Results.NotFound();
        }

        dbContext.JobSchedules.Remove(schedule);
        await dbContext.SaveChangesAsync(cancellationToken);

        await auditTrailService.RecordAsync(
            new AuditRecordRequest(
                userContext.UserId.ToString("N"),
                "scheduler.schedule.delete",
                "JobSchedule",
                schedule.Id.ToString("N"),
                "{}"),
            cancellationToken);

        return Results.Ok(new { deleted = true });
    }
}

