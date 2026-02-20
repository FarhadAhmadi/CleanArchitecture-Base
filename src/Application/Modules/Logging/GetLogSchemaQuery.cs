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

public sealed record GetLogSchemaQuery : IQuery<IResult>;
internal sealed class GetLogSchemaQueryHandler : ResultWrappingQueryHandler<GetLogSchemaQuery>
{
    private static readonly string[] RequiredSchemaFields =
    [
        "eventId", "timestampUtc", "level", "message", "source.service",
        "source.module", "traceId", "requestId", "tenantId", "actor.type",
        "actor.id", "outcome"
    ];

    protected override async Task<IResult> HandleCore(GetLogSchemaQuery query, CancellationToken cancellationToken) =>
        Results.Ok(new { version = "1.0", required = RequiredSchemaFields });
}





