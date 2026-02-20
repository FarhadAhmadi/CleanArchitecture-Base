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

public sealed record GetAlertIncidentByIdQuery(Guid Id) : IQuery<IResult>;
internal sealed class GetAlertIncidentByIdQueryHandler(IApplicationReadDbContext readContext) : ResultWrappingQueryHandler<GetAlertIncidentByIdQuery>
{
    protected override async Task<IResult> HandleCore(GetAlertIncidentByIdQuery query, CancellationToken cancellationToken)
    {
        AlertIncident? incident = await readContext.AlertIncidents
            .SingleOrDefaultAsync(x => x.Id == query.Id, cancellationToken);
        if (incident is null)
        {
            return Results.NotFound();
        }

        AlertRule? rule = await readContext.AlertRules
            .SingleOrDefaultAsync(x => x.Id == incident.RuleId, cancellationToken);
        LogEvent? logEvent = await readContext.LogEvents
            .SingleOrDefaultAsync(x => x.Id == incident.TriggerEventId, cancellationToken);

        return Results.Ok(new
        {
            incident.Id,
            incident.RuleId,
            ruleName = rule?.Name,
            incident.TriggerEventId,
            incident.TriggeredAtUtc,
            incident.Status,
            incident.RetryCount,
            incident.NextRetryAtUtc,
            incident.LastError,
            trigger = logEvent is null
                ? null
                : new
                {
                    logEvent.Id,
                    logEvent.Level,
                    logEvent.Message,
                    logEvent.SourceService,
                    logEvent.SourceModule,
                    logEvent.TraceId,
                    logEvent.Outcome,
                    logEvent.TimestampUtc
                }
        });
    }
}





