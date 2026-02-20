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

public sealed record GetAlertRulesQuery : IQuery<IResult>;
internal sealed class GetAlertRulesQueryHandler(IApplicationReadDbContext readContext) : ResultWrappingQueryHandler<GetAlertRulesQuery>
{
    protected override async Task<IResult> HandleCore(GetAlertRulesQuery query, CancellationToken cancellationToken)
    {
        List<AlertRule> rules = await readContext.AlertRules.OrderBy(x => x.Name).ToListAsync(cancellationToken);
        return Results.Ok(rules);
    }
}





