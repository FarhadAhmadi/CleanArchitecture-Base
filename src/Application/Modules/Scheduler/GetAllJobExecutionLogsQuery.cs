using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Scheduler;
using Microsoft.EntityFrameworkCore;

namespace Application.Scheduler;

public sealed record GetAllJobExecutionLogsQuery(
    int? Page,
    int? PageIndex,
    int? PageSize,
    Guid? JobId,
    JobExecutionStatus? Status,
    DateTime? From,
    DateTime? To) : IQuery<IResult>;

internal sealed class GetAllJobExecutionLogsQueryHandler(IApplicationReadDbContext readDbContext) : ResultWrappingQueryHandler<GetAllJobExecutionLogsQuery>
{
    protected override async Task<IResult> HandleCore(GetAllJobExecutionLogsQuery query, CancellationToken cancellationToken)
    {
        int page = query.PageIndex ?? query.Page ?? 1;
        int pageSize = query.PageSize ?? 50;
        (int normalizedPage, int normalizedPageSize) = QueryableExtensions.NormalizePaging(page, pageSize, 50, 200);

        IQueryable<Domain.Modules.Scheduler.JobExecution> readQuery = readDbContext.JobExecutions;

        if (query.JobId.HasValue)
        {
            readQuery = readQuery.Where(x => x.JobId == query.JobId.Value);
        }

        if (query.Status.HasValue)
        {
            readQuery = readQuery.Where(x => x.Status == query.Status.Value);
        }

        if (query.From.HasValue)
        {
            readQuery = readQuery.Where(x => x.StartedAtUtc >= query.From.Value);
        }

        if (query.To.HasValue)
        {
            readQuery = readQuery.Where(x => x.StartedAtUtc <= query.To.Value);
        }

        readQuery = readQuery.OrderByDescending(x => x.StartedAtUtc);

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

        return Results.Ok(new { page = normalizedPage, pageSize = normalizedPageSize, total, items });
    }
}
