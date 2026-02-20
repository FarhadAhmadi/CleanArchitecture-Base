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

public sealed record IngestSingleLogCommand(IngestLogRequest Request, string? IdempotencyKey) : ICommand<IResult>;
internal sealed class IngestSingleLogCommandHandler(ILogIngestionService ingestionService) : ResultWrappingCommandHandler<IngestSingleLogCommand>
{
    protected override async Task<IResult> HandleCore(IngestSingleLogCommand command, CancellationToken cancellationToken)
    {
        string? idempotencyKey = InputSanitizer.SanitizeIdentifier(command.IdempotencyKey, 120);
        IngestResult result = await ingestionService.IngestAsync(command.Request, idempotencyKey, cancellationToken);
        return Results.Ok(result);
    }
}





