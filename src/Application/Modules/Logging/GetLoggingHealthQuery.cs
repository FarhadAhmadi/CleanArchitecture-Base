using System.Security.Claims;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Authorization;
using Domain.Logging;
using Application.Abstractions.Auditing;
using Application.Abstractions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedKernel;
using Application.Shared;

namespace Application.Logging;

public sealed record GetLoggingHealthQuery : IQuery<IResult>;
internal sealed class GetLoggingHealthQueryHandler(ILoggingHealthService healthService) : ResultWrappingQueryHandler<GetLoggingHealthQuery>
{
    protected override async Task<IResult> HandleCore(GetLoggingHealthQuery query, CancellationToken cancellationToken) =>
        Results.Ok(healthService.GetHealth());
}





