using System.Security.Claims;
using Application.Abstractions.Messaging;
using Application.Authorization.Roles;
using Infrastructure.Auditing;
using SharedKernel;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Authorization;

internal sealed class DeleteRole : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("authorization/roles/{roleId:guid}", async (
            Guid roleId,
            HttpContext httpContext,
            IAuditTrailService auditTrailService,
            ICommandHandler<DeleteRoleCommand> handler,
            CancellationToken cancellationToken) =>
        {
            Result result = await handler.Handle(new DeleteRoleCommand(roleId), cancellationToken);

            if (result.IsSuccess)
            {
                string actorId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
                await auditTrailService.RecordAsync(
                    new AuditRecordRequest(
                        actorId,
                        "authorization.role.delete",
                        "Role",
                        roleId.ToString("N"),
                        "{}"),
                    cancellationToken);
            }

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.Users)
        .HasPermission(Permissions.AuthorizationManage);
    }
}
