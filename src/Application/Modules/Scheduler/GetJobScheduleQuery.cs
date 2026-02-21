using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;

namespace Application.Scheduler;

public sealed record GetJobScheduleQuery(Guid JobId) : IQuery<IResult>;

internal sealed class GetJobScheduleQueryHandler(IApplicationReadDbContext readDbContext) : ResultWrappingQueryHandler<GetJobScheduleQuery>
{
    protected override async Task<IResult> HandleCore(GetJobScheduleQuery query, CancellationToken cancellationToken)
    {
        object? item = await readDbContext.JobSchedules
            .Where(x => x.JobId == query.JobId)
            .Select(x => new
            {
                x.Id,
                x.JobId,
                type = x.Type.ToString(),
                x.CronExpression,
                x.IntervalSeconds,
                x.OneTimeAtUtc,
                x.StartAtUtc,
                x.EndAtUtc,
                x.NextRunAtUtc,
                x.IsEnabled,
                misfirePolicy = x.MisfirePolicy.ToString(),
                x.MaxCatchUpRuns,
                x.RetryAttempt,
                x.LastMisfireAtUtc
            })
            .SingleOrDefaultAsync(cancellationToken);

        return item is null ? Results.NotFound() : Results.Ok(item);
    }
}
