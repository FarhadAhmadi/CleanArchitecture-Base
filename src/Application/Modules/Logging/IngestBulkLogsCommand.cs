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

public sealed record IngestBulkLogsCommand(BulkIngestRequest Request, string? IdempotencyKey) : ICommand<IResult>;
internal sealed class IngestBulkLogsCommandHandler(ILogIngestionService ingestionService, LoggingOptions options) : ResultWrappingCommandHandler<IngestBulkLogsCommand>
{
    protected override async Task<IResult> HandleCore(IngestBulkLogsCommand command, CancellationToken cancellationToken)
    {
        if (command.Request.Events.Count == 0 || command.Request.Events.Count > options.MaxBulkItems)
        {
            return Results.Problem(
                title: "Bad request",
                detail: $"Bulk size must be between 1 and {options.MaxBulkItems}.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        List<IngestResult> results = [];
        for (int i = 0; i < command.Request.Events.Count; i++)
        {
            string? key = InputSanitizer.SanitizeIdentifier(command.IdempotencyKey, 100);
            if (!string.IsNullOrWhiteSpace(key))
            {
                key = $"{key}:{i}";
            }

            IngestResult result = await ingestionService.IngestAsync(command.Request.Events[i], key, cancellationToken);
            results.Add(result);
        }

        return Results.Ok(results);
    }
}





