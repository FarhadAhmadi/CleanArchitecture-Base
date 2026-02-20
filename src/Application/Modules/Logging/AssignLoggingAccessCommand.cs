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

public sealed record AssignLoggingAccessCommand(AssignAccessRequest Request) : ICommand<IResult>;
internal sealed class AssignLoggingAccessCommandHandler(
    IApplicationDbContext writeContext,
    IAuditTrailService auditTrailService,
    IHttpContextAccessor httpContextAccessor) : ResultWrappingCommandHandler<AssignLoggingAccessCommand>
{
    protected override async Task<IResult> HandleCore(AssignLoggingAccessCommand command, CancellationToken cancellationToken)
    {
        command.Request.RoleName = InputSanitizer.SanitizeIdentifier(command.Request.RoleName, 100) ?? string.Empty;
        command.Request.PermissionCode = InputSanitizer.SanitizeIdentifier(command.Request.PermissionCode, 200) ?? string.Empty;

        Role? role = await writeContext.Roles.SingleOrDefaultAsync(x => x.Name == command.Request.RoleName, cancellationToken);
        if (role is null)
        {
            return Results.NotFound(new { error = "Role not found" });
        }

        Permission? permission = await writeContext.Permissions
            .SingleOrDefaultAsync(x => x.Code == command.Request.PermissionCode, cancellationToken);
        if (permission is null)
        {
            return Results.NotFound(new { error = "Permission not found" });
        }

        bool exists = await writeContext.RolePermissions.AnyAsync(
            x => x.RoleId == role.Id && x.PermissionId == permission.Id,
            cancellationToken);

        if (!exists)
        {
            writeContext.RolePermissions.Add(command.Request.ToEntity(role.Id, permission.Id));
            await writeContext.SaveChangesAsync(cancellationToken);

            string actorId = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
            await auditTrailService.RecordAsync(
                new AuditRecordRequest(
                    actorId,
                    "logging.access.assign",
                    "RolePermission",
                    role.Id.ToString("N"),
                    $"{{\"permission\":\"{permission.Code}\"}}"),
                cancellationToken);
        }

        return Results.NoContent();
    }
}





