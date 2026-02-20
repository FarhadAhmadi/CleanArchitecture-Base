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

public sealed record CreateLoggingRoleCommand(CreateRoleRequest Request) : ICommand<IResult>;
internal sealed class CreateLoggingRoleCommandHandler(
    IApplicationDbContext writeContext,
    IAuditTrailService auditTrailService,
    IHttpContextAccessor httpContextAccessor) : ResultWrappingCommandHandler<CreateLoggingRoleCommand>
{
    protected override async Task<IResult> HandleCore(CreateLoggingRoleCommand command, CancellationToken cancellationToken)
    {
        command.Request.RoleName = InputSanitizer.SanitizeIdentifier(command.Request.RoleName, 100) ?? string.Empty;

        bool exists = await writeContext.Roles.AnyAsync(x => x.Name == command.Request.RoleName, cancellationToken);
        if (exists)
        {
            return Results.Ok();
        }

        Role role = command.Request.ToEntity();
        writeContext.Roles.Add(role);
        await writeContext.SaveChangesAsync(cancellationToken);

        string actorId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
        await auditTrailService.RecordAsync(
            new AuditRecordRequest(
                actorId,
                "logging.access-role.create",
                "Role",
                role.Id.ToString("N"),
                $"{{\"name\":\"{role.Name}\"}}"),
            cancellationToken);

        return Results.NoContent();
    }
}





