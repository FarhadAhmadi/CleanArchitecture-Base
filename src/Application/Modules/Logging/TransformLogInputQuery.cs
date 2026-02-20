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

public sealed record TransformLogInputQuery(IngestLogRequest Request) : IQuery<IResult>;
internal sealed class TransformLogInputQueryHandler(ILogSanitizer sanitizer) : ResultWrappingQueryHandler<TransformLogInputQuery>
{
    protected override async Task<IResult> HandleCore(TransformLogInputQuery query, CancellationToken cancellationToken)
    {
        IngestLogRequest transformed = sanitizer.Sanitize(query.Request);
        return Results.Ok(transformed);
    }
}





