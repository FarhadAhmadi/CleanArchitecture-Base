using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;

namespace Application.Scheduler;

public sealed record GetJobExecutionLogsQuery(Guid JobId, int? Page, int? PageIndex, int? PageSize) : IQuery<IResult>;

internal sealed class GetJobExecutionLogsQueryHandler(IApplicationReadDbContext readDbContext) : ResultWrappingQueryHandler<GetJobExecutionLogsQuery>
{
    protected override async Task<IResult> HandleCore(GetJobExecutionLogsQuery query, CancellationToken cancellationToken)
    {
        int page = query.PageIndex ?? query.Page ?? 1;
        int pageSize = query.PageSize ?? 50;
        (int normalizedPage, int normalizedPageSize) = QueryableExtensions.NormalizePaging(page, pageSize, 50, 200);

        IQueryable<Domain.Modules.Scheduler.JobExecution> readQuery = readDbContext.JobExecutions
            .Where(x => x.JobId == query.JobId)
            .OrderByDescending(x => x.StartedAtUtc);

        int total = await readQuery.CountAsync(cancellationToken);
        List<object> items = await readQuery
            .ApplyPaging(normalizedPage, normalizedPageSize)
            .Select(x => new
            {
                x.Id,
                x.JobId,
                status = x.Status.ToString(),
                x.TriggeredBy,
                x.NodeId,
                x.ScheduledAtUtc,
                x.StartedAtUtc,
                x.FinishedAtUtc,
                x.DurationMs,
                x.Attempt,
                x.MaxAttempts,
                x.IsReplay,
                x.IsDeadLetter,
                x.DeadLetterReason,
                x.Error
            })
            .Cast<object>()
            .ToListAsync(cancellationToken);

        return Results.Ok(new
        {
            page = normalizedPage,
            pageSize = normalizedPageSize,
            total,
            items
        });
    }
}
