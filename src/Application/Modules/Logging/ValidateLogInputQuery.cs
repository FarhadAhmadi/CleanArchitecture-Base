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

public sealed record ValidateLogInputQuery(IngestLogRequest Request) : IQuery<IResult>;
internal sealed class ValidateLogInputQueryHandler : ResultWrappingQueryHandler<ValidateLogInputQuery>
{
    protected override async Task<IResult> HandleCore(ValidateLogInputQuery query, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query.Request.Message))
        {
            return Results.Problem(
                title: "Bad request",
                detail: "Message is required.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        if (string.IsNullOrWhiteSpace(query.Request.SourceService) ||
            string.IsNullOrWhiteSpace(query.Request.SourceModule))
        {
            return Results.Problem(
                title: "Bad request",
                detail: "SourceService and SourceModule are required.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        return Results.Ok(new { valid = true });
    }
}





