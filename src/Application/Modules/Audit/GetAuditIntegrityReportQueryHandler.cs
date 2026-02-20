using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Auditing;
using Infrastructure.Auditing;
using Microsoft.EntityFrameworkCore;

namespace Application.Audit;

public sealed record GetAuditIntegrityReportQuery(bool UpdateFlags) : IQuery<IResult>;
internal sealed class GetAuditIntegrityReportQueryHandler(
    IApplicationReadDbContext readContext,
    IApplicationDbContext writeContext,
    IAuditTrailService auditTrailService) : ResultWrappingQueryHandler<GetAuditIntegrityReportQuery>
{
    protected override async Task<IResult> HandleCore(GetAuditIntegrityReportQuery query, CancellationToken cancellationToken)
    {
        List<AuditEntry> entries = await readContext.AuditEntries
            .OrderBy(x => x.TimestampUtc)
            .ThenBy(x => x.Id)
            .Take(10000)
            .ToListAsync(cancellationToken);

        List<Guid> tamperedIds = [];
        foreach (AuditEntry entry in entries)
        {
            bool isTampered = await auditTrailService.IsTamperedAsync(entry, cancellationToken) || entry.IsTampered;
            if (isTampered)
            {
                tamperedIds.Add(entry.Id);
            }
        }

        if (query.UpdateFlags && tamperedIds.Count != 0)
        {
            List<AuditEntry> tracked = await writeContext.AuditEntries
                .Where(x => tamperedIds.Contains(x.Id) && !x.IsTampered)
                .ToListAsync(cancellationToken);

            foreach (AuditEntry entry in tracked)
            {
                entry.IsTampered = true;
            }

            await writeContext.SaveChangesAsync(cancellationToken);
        }

        return Results.Ok(new
        {
            totalChecked = entries.Count,
            tampered = tamperedIds.Count,
            tamperedIds
        });
    }
}




