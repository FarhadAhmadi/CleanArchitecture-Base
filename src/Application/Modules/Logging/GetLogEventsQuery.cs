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

public sealed record GetLogEventsQuery(GetLogEventsRequest Request) : IQuery<IResult>;
internal sealed class GetLogEventsQueryHandler(
    IApplicationReadDbContext readContext,
    IApplicationDbContext writeContext,
    ILogIntegrityService integrityService,
    ILoggerFactory loggerFactory) : ResultWrappingQueryHandler<GetLogEventsQuery>
{
    protected override async Task<IResult> HandleCore(GetLogEventsQuery query, CancellationToken cancellationToken)
    {
        ILogger logger = loggerFactory.CreateLogger("LoggingEndpoints.GetEvents");

        (int normalizedPage, int normalizedPageSize) = query.Request.NormalizePaging();

        IQueryable<LogEvent> readQuery = readContext.LogEvents.Where(x => !x.IsDeleted);

        if (query.Request.Level.HasValue)
        {
            readQuery = readQuery.Where(x => x.Level == query.Request.Level.Value);
        }

        if (query.Request.From.HasValue)
        {
            readQuery = readQuery.Where(x => x.TimestampUtc >= query.Request.From.Value);
        }

        if (query.Request.To.HasValue)
        {
            readQuery = readQuery.Where(x => x.TimestampUtc <= query.Request.To.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Request.ActorId))
        {
            readQuery = readQuery.Where(x => x.ActorId == query.Request.ActorId);
        }

        if (!string.IsNullOrWhiteSpace(query.Request.Service))
        {
            readQuery = readQuery.Where(x => x.SourceService == query.Request.Service);
        }

        if (!string.IsNullOrWhiteSpace(query.Request.Module))
        {
            readQuery = readQuery.Where(x => x.SourceModule == query.Request.Module);
        }

        if (!string.IsNullOrWhiteSpace(query.Request.TraceId))
        {
            readQuery = readQuery.Where(x => x.TraceId == query.Request.TraceId);
        }

        if (!string.IsNullOrWhiteSpace(query.Request.Outcome))
        {
            readQuery = readQuery.Where(x => x.Outcome == query.Request.Outcome);
        }

        readQuery = readQuery.ApplyContainsSearch(query.Request.Text, x => x.Message, x => x.PayloadJson, x => x.TagsCsv);
        readQuery = readQuery.ApplySorting(query.Request.SortBy, query.Request.SortOrder);

        int total = await readQuery.CountAsync(cancellationToken);
        List<LogEvent> items = await readQuery
            .ApplyPaging(normalizedPage, normalizedPageSize)
            .ToListAsync(cancellationToken);

        List<Guid> corruptedIds = [];
        List<LogEventView> resultItems = [];

        foreach (LogEvent item in items)
        {
            bool isCorrupted = integrityService.IsCorrupted(item) || item.HasIntegrityIssue;
            if (isCorrupted && !item.HasIntegrityIssue)
            {
                corruptedIds.Add(item.Id);
            }

            resultItems.Add(item.ToView(isCorrupted));
        }

        if (query.Request.RecalculateIntegrity && corruptedIds.Count != 0)
        {
            List<LogEvent> trackedItems = await writeContext.LogEvents
                .Where(x => corruptedIds.Contains(x.Id) && !x.HasIntegrityIssue)
                .ToListAsync(cancellationToken);

            foreach (LogEvent trackedItem in trackedItems)
            {
                trackedItem.HasIntegrityIssue = true;
            }

            await writeContext.SaveChangesAsync(cancellationToken);
        }

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Log events queried. Total={Total} Returned={Returned} Page={Page} PageSize={PageSize} Level={Level}",
                total,
                resultItems.Count,
                normalizedPage,
                normalizedPageSize,
                query.Request.Level);
        }

        return Results.Ok(new
        {
            page = normalizedPage,
            pageSize = normalizedPageSize,
            total,
            items = resultItems
        });
    }
}





