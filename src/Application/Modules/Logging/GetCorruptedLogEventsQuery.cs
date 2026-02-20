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

public sealed record GetCorruptedLogEventsQuery(bool Recalculate) : IQuery<IResult>;
internal sealed class GetCorruptedLogEventsQueryHandler(
    IApplicationReadDbContext readContext,
    IApplicationDbContext writeContext,
    ILogIntegrityService integrityService,
    ILoggerFactory loggerFactory) : ResultWrappingQueryHandler<GetCorruptedLogEventsQuery>
{
    protected override async Task<IResult> HandleCore(GetCorruptedLogEventsQuery query, CancellationToken cancellationToken)
    {
        ILogger logger = loggerFactory.CreateLogger("LoggingEndpoints.GetCorruptedEvents");

        List<LogEvent> events = await readContext.LogEvents
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.TimestampUtc)
            .Take(5000)
            .ToListAsync(cancellationToken);

        List<LogEventView> corrupted = [];

        foreach (LogEvent logEvent in events)
        {
            bool mismatch = integrityService.IsCorrupted(logEvent) || logEvent.HasIntegrityIssue;
            if (!mismatch)
            {
                continue;
            }

            corrupted.Add(logEvent.ToView(true));

            if (query.Recalculate && !logEvent.HasIntegrityIssue)
            {
                LogEvent? tracked = await writeContext.LogEvents
                    .SingleOrDefaultAsync(x => x.Id == logEvent.Id, cancellationToken);

                tracked?.HasIntegrityIssue = true;
            }
        }

        if (query.Recalculate)
        {
            await writeContext.SaveChangesAsync(cancellationToken);
        }

        if (logger.IsEnabled(LogLevel.Warning))
        {
            logger.LogWarning(
                "Corrupted log events queried. TotalChecked={TotalChecked} Corrupted={Corrupted} Recalculated={Recalculated}",
                events.Count,
                corrupted.Count,
                query.Recalculate);
        }

        return Results.Ok(new { total = corrupted.Count, items = corrupted });
    }
}





