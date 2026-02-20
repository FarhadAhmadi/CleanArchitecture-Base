using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Auditing;
using Microsoft.EntityFrameworkCore;

namespace Application.Audit;

public sealed record GetAuditEntriesQuery(GetAuditEntriesRequest Request) : IQuery<IResult>;
internal sealed class GetAuditEntriesQueryHandler(IApplicationReadDbContext readContext) : ResultWrappingQueryHandler<GetAuditEntriesQuery>
{
    protected override async Task<IResult> HandleCore(GetAuditEntriesQuery query, CancellationToken cancellationToken)
    {
        (int normalizedPage, int normalizedPageSize) = query.Request.NormalizePaging();

        IQueryable<AuditEntry> readQuery = readContext.AuditEntries;

        if (!string.IsNullOrWhiteSpace(query.Request.ActorId))
        {
            readQuery = readQuery.Where(x => x.ActorId == query.Request.ActorId);
        }

        if (!string.IsNullOrWhiteSpace(query.Request.Action))
        {
            readQuery = readQuery.Where(x => x.Action == query.Request.Action);
        }

        if (query.Request.From.HasValue)
        {
            readQuery = readQuery.Where(x => x.TimestampUtc >= query.Request.From.Value);
        }

        if (query.Request.To.HasValue)
        {
            readQuery = readQuery.Where(x => x.TimestampUtc <= query.Request.To.Value);
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




