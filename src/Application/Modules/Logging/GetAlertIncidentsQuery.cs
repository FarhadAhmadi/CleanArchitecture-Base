using System.Security.Claims;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Authorization;
using Domain.Logging;
using Infrastructure.Auditing;
using Infrastructure.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;
using Application.Shared;

namespace Application.Logging;

public sealed record GetAlertIncidentsQuery(int Page, int PageSize, string? Status) : IQuery<IResult>;
internal sealed class GetAlertIncidentsQueryHandler(IApplicationReadDbContext readContext) : ResultWrappingQueryHandler<GetAlertIncidentsQuery>
{
    protected override async Task<IResult> HandleCore(GetAlertIncidentsQuery query, CancellationToken cancellationToken)
    {
        int normalizedPage = Math.Max(1, query.Page);
        int normalizedPageSize = Math.Clamp(query.PageSize <= 0 ? 50 : query.PageSize, 1, 200);

        IQueryable<AlertIncident> readQuery = readContext.AlertIncidents;
        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            readQuery = readQuery.Where(x => x.Status == query.Status);
        }

        int total = await readQuery.CountAsync(cancellationToken);
        List<AlertIncident> incidents = await readQuery
            .OrderByDescending(x => x.TriggeredAtUtc)
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .ToListAsync(cancellationToken);

        var ruleIds = incidents.Select(x => x.RuleId).Distinct().ToList();
        var eventIds = incidents.Select(x => x.TriggerEventId).Distinct().ToList();

        Dictionary<Guid, AlertRule> rules = await readContext.AlertRules
            .Where(x => ruleIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        Dictionary<Guid, LogEvent> events = await readContext.LogEvents
            .Where(x => eventIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        IEnumerable<object> items = incidents.Select(x =>
        {
            rules.TryGetValue(x.RuleId, out AlertRule? rule);
            events.TryGetValue(x.TriggerEventId, out LogEvent? logEvent);

            return new
            {
                x.Id,
                x.RuleId,
                ruleName = rule?.Name,
                x.TriggerEventId,
                x.TriggeredAtUtc,
                x.Status,
                x.RetryCount,
                x.NextRetryAtUtc,
                x.LastError,
                level = logEvent?.Level,
                message = logEvent?.Message,
                sourceService = logEvent?.SourceService,
                sourceModule = logEvent?.SourceModule,
                traceId = logEvent?.TraceId
            };
        });

        return Results.Ok(new
        {
            page = normalizedPage,
            pageSize = normalizedPageSize,
            total,
            items
        });
    }
}





