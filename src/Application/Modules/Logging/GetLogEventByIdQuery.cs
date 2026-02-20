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

public sealed record GetLogEventByIdQuery(Guid EventId, bool RecalculateIntegrity) : IQuery<IResult>;
internal sealed class GetLogEventByIdQueryHandler(
    IApplicationReadDbContext readContext,
    IApplicationDbContext writeContext,
    ILogIntegrityService integrityService) : ResultWrappingQueryHandler<GetLogEventByIdQuery>
{
    protected override async Task<IResult> HandleCore(GetLogEventByIdQuery query, CancellationToken cancellationToken)
    {
        LogEvent? item = await readContext.LogEvents
            .SingleOrDefaultAsync(x => x.Id == query.EventId && !x.IsDeleted, cancellationToken);

        if (item is null)
        {
            return Results.NotFound();
        }

        bool isCorrupted = integrityService.IsCorrupted(item) || item.HasIntegrityIssue;
        if (query.RecalculateIntegrity && isCorrupted && !item.HasIntegrityIssue)
        {
            LogEvent? tracked = await writeContext.LogEvents.SingleOrDefaultAsync(x => x.Id == item.Id, cancellationToken);
            if (tracked is not null)
            {
                tracked.HasIntegrityIssue = true;
                await writeContext.SaveChangesAsync(cancellationToken);
            }
        }

        return Results.Ok(item.ToView(isCorrupted));
    }
}





