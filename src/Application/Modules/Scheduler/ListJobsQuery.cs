using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Scheduler;
using Microsoft.EntityFrameworkCore;

namespace Application.Scheduler;

public sealed record ListJobsQuery(
    int? Page,
    int? PageIndex,
    int? PageSize,
    JobStatus? Status,
    string? Search) : IQuery<IResult>;

internal sealed class ListJobsQueryHandler(IApplicationReadDbContext readDbContext) : ResultWrappingQueryHandler<ListJobsQuery>
{
    protected override async Task<IResult> HandleCore(ListJobsQuery query, CancellationToken cancellationToken)
    {
        int page = query.PageIndex ?? query.Page ?? 1;
        int pageSize = query.PageSize ?? 50;
        (int normalizedPage, int normalizedPageSize) = QueryableExtensions.NormalizePaging(page, pageSize, 50, 200);

        IQueryable<Domain.Modules.Scheduler.ScheduledJob> readQuery = readDbContext.ScheduledJobs;

        if (query.Status.HasValue)
        {
            readQuery = readQuery.Where(x => x.Status == query.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            readQuery = readQuery.ApplyContainsSearch(query.Search, x => x.Name, x => x.Description);
        }

        readQuery = readQuery.OrderByDescending(x => x.CreatedAtUtc);

        int total = await readQuery.CountAsync(cancellationToken);
        List<object> items = await readQuery
            .ApplyPaging(normalizedPage, normalizedPageSize)
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
                x.QuarantinedUntilUtc
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
