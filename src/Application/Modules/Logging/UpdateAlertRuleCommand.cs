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

public sealed record UpdateAlertRuleCommand(Guid Id, UpdateAlertRuleRequest Request) : ICommand<IResult>;
internal sealed class UpdateAlertRuleCommandHandler(
    IApplicationDbContext writeContext,
    IAuditTrailService auditTrailService,
    IHttpContextAccessor httpContextAccessor) : ResultWrappingCommandHandler<UpdateAlertRuleCommand>
{
    protected override async Task<IResult> HandleCore(UpdateAlertRuleCommand command, CancellationToken cancellationToken)
    {
        command.Request.Sanitize();
        AlertRule? rule = await writeContext.AlertRules.SingleOrDefaultAsync(x => x.Id == command.Id, cancellationToken);
        if (rule is null)
        {
            return Results.NotFound();
        }

        command.Request.Update(rule);
        await writeContext.SaveChangesAsync(cancellationToken);

        string actorId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
        await auditTrailService.RecordAsync(
            new AuditRecordRequest(
                actorId,
                "logging.alert-rule.update",
                "AlertRule",
                rule.Id.ToString("N"),
                $"{{\"name\":\"{rule.Name}\",\"minimumLevel\":\"{rule.MinimumLevel}\"}}"),
            cancellationToken);

        return Results.Ok(rule);
    }
}





