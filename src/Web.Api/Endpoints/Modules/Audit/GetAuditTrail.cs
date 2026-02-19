using Application.Abstractions.Data;
using Domain.Auditing;
using Infrastructure.Auditing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Audit;

internal sealed class GetAuditTrail : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("audit", GetEntries)
            .WithTags("Audit")
            .HasPermission(Permissions.AuditRead);

        app.MapGet("audit/integrity", GetIntegrityReport)
            .WithTags("Audit")
            .HasPermission(Permissions.AuditRead);
    }

    private static async Task<IResult> GetEntries(
        IApplicationReadDbContext readContext,
        [AsParameters] GetAuditEntriesRequest request,
        CancellationToken cancellationToken)
    {
        (int normalizedPage, int normalizedPageSize) = request.NormalizePaging();

        IQueryable<AuditEntry> query = readContext.AuditEntries;

        if (!string.IsNullOrWhiteSpace(request.ActorId))
        {
            query = query.Where(x => x.ActorId == request.ActorId);
        }

        if (!string.IsNullOrWhiteSpace(request.Action))
        {
            query = query.Where(x => x.Action == request.Action);
        }

        if (request.From.HasValue)
        {
            query = query.Where(x => x.TimestampUtc >= request.From.Value);
        }

        if (request.To.HasValue)
        {
            query = query.Where(x => x.TimestampUtc <= request.To.Value);
        }

        query = query.OrderByDescending(x => x.TimestampUtc);

        int total = await query.CountAsync(cancellationToken);
        List<object> items = await query
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

    private static async Task<IResult> GetIntegrityReport(
        IApplicationReadDbContext readContext,
        IApplicationDbContext writeContext,
        IAuditTrailService auditTrailService,
        bool updateFlags,
        CancellationToken cancellationToken)
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

        if (updateFlags && tamperedIds.Count != 0)
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
