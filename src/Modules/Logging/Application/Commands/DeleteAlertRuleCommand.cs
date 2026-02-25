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

public sealed record DeleteAlertRuleCommand(Guid Id) : ICommand<IResult>;
internal sealed class DeleteAlertRuleCommandHandler(
    ILoggingWriteDbContext writeContext,
    IAuditTrailService auditTrailService,
    IHttpContextAccessor httpContextAccessor) : ResultWrappingCommandHandler<DeleteAlertRuleCommand>
{
    protected override async Task<IResult> HandleCore(DeleteAlertRuleCommand command, CancellationToken cancellationToken)
    {
        AlertRule? rule = await writeContext.AlertRules.SingleOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
        if (rule is null)
        {
            return Results.NotFound();
        }

        writeContext.AlertRules.Remove(rule);
        await writeContext.SaveChangesAsync(cancellationToken);

        string actorId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
        await auditTrailService.RecordAsync(
            new AuditRecordRequest(
                actorId,
                "logging.alert-rule.delete",
                "AlertRule",
                command.Id.ToString("N"),
                "{}"),
            cancellationToken);

        return Results.NoContent();
    }
}







