using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Auditing;
using Microsoft.EntityFrameworkCore;

namespace Application.Audit;

public sealed record GetAuditEntriesQuery(
    int? Page,
    int? PageIndex,
    int? PageSize,
    string? ActorId,
    string? Action,
    DateTime? From,
    DateTime? To) : IQuery<IResult>;

internal sealed class GetAuditEntriesQueryHandler(IApplicationReadDbContext readContext) : ResultWrappingQueryHandler<GetAuditEntriesQuery>
{
    protected override async Task<IResult> HandleCore(GetAuditEntriesQuery query, CancellationToken cancellationToken)
    {
        int page = query.PageIndex ?? query.Page ?? 1;
        int pageSize = query.PageSize ?? 50;
        (int normalizedPage, int normalizedPageSize) = QueryableExtensions.NormalizePaging(page, pageSize, 50, 200);

        IQueryable<AuditEntry> readQuery = readContext.AuditEntries;

        if (!string.IsNullOrWhiteSpace(query.ActorId))
        {
            readQuery = readQuery.Where(x => x.ActorId == query.ActorId);
        }

        if (!string.IsNullOrWhiteSpace(query.Action))
        {
            readQuery = readQuery.Where(x => x.Action == query.Action);
        }

        if (query.From.HasValue)
        {
            readQuery = readQuery.Where(x => x.TimestampUtc >= query.From.Value);
        }

        if (query.To.HasValue)
        {
            readQuery = readQuery.Where(x => x.TimestampUtc <= query.To.Value);
        }

        readQuery = readQuery.OrderByDescending(x => x.TimestampUtc);

        int total = await readQuery.CountAsync(cancellationToken);
        List<object> items = await readQuery
            .ApplyPaging(normalizedPage, normalizedPageSize)
            .Select(x => new
            {
                x.Id,
                x.TimestampUtc,
                x.ActorId,
                x.Action,
                x.ResourceType,
                x.ResourceId,
                x.IsTampered
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




