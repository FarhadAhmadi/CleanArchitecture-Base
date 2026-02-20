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

public sealed record CreateAlertRuleCommand(CreateAlertRuleRequest Request) : ICommand<IResult>;
internal sealed class CreateAlertRuleCommandHandler(
    IApplicationDbContext writeContext,
    IAuditTrailService auditTrailService,
    IHttpContextAccessor httpContextAccessor) : ResultWrappingCommandHandler<CreateAlertRuleCommand>
{
    protected override async Task<IResult> HandleCore(CreateAlertRuleCommand command, CancellationToken cancellationToken)
    {
        command.Request.Sanitize();
        AlertRule entity = command.Request.ToEntity();
        writeContext.AlertRules.Add(entity);
        await writeContext.SaveChangesAsync(cancellationToken);

        string actorId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
        await auditTrailService.RecordAsync(
            new AuditRecordRequest(
                actorId,
                "logging.alert-rule.create",
                "AlertRule",
                entity.Id.ToString("N"),
                $"{{\"name\":\"{entity.Name}\",\"minimumLevel\":\"{entity.MinimumLevel}\"}}"),
            cancellationToken);

        return Results.Ok(entity);
    }
}





