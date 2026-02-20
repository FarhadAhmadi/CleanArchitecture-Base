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

public sealed record DeleteLogEventCommand(Guid EventId) : ICommand<IResult>;
internal sealed class DeleteLogEventCommandHandler(
    IApplicationDbContext writeContext,
    IAuditTrailService auditTrailService,
    IHttpContextAccessor httpContextAccessor) : ResultWrappingCommandHandler<DeleteLogEventCommand>
{
    protected override async Task<IResult> HandleCore(DeleteLogEventCommand command, CancellationToken cancellationToken)
    {
        LogEvent? item = await writeContext.LogEvents.SingleOrDefaultAsync(x => x.Id == command.EventId, cancellationToken);
        if (item is null)
        {
            return Results.NotFound();
        }

        item.IsDeleted = true;
        item.DeletedAtUtc = DateTime.UtcNow;
        await writeContext.SaveChangesAsync(cancellationToken);

        string actorId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
        await auditTrailService.RecordAsync(
            new AuditRecordRequest(
                actorId,
                "logging.event.delete",
                "LogEvent",
                command.EventId.ToString("N"),
                "{\"softDelete\":true}"),
            cancellationToken);

        return Results.NoContent();
    }
}





