using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;

namespace Application.Scheduler;

public sealed record GetJobByIdQuery(Guid JobId) : IQuery<IResult>;

internal sealed class GetJobByIdQueryHandler(IApplicationReadDbContext readDbContext) : ResultWrappingQueryHandler<GetJobByIdQuery>
{
    protected override async Task<IResult> HandleCore(GetJobByIdQuery query, CancellationToken cancellationToken)
    {
        object? item = await readDbContext.ScheduledJobs
            .Where(x => x.Id == query.JobId)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.Description,
                type = x.Type.ToString(),
                x.PayloadJson,
                status = x.Status.ToString(),
                x.CreatedAtUtc,
                x.LastRunAtUtc,
                lastExecutionStatus = x.LastExecutionStatus.HasValue ? x.LastExecutionStatus.Value.ToString() : null,
                x.MaxRetryAttempts,
                x.RetryBackoffSeconds,
                x.MaxExecutionSeconds,
                x.MaxConsecutiveFailures,
                x.ConsecutiveFailures,
                x.IsQuarantined,
                x.QuarantinedUntilUtc,
                x.LastFailureAtUtc,
                x.DeadLetterReason
            })
            .SingleOrDefaultAsync(cancellationToken);

        return item is null ? Results.NotFound() : Results.Ok(item);
    }
}
