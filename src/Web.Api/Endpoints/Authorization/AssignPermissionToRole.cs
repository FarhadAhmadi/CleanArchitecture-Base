using Application.Abstractions.Messaging;
using Application.Authorization.AssignPermissionToRole;
using Infrastructure.Auditing;
using SharedKernel;
using System.Security.Claims;
using Web.Api.Endpoints.Mappings;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Authorization;

internal sealed class AssignPermissionToRole : IEndpoint
{
    public sealed record Request(string RoleName, string PermissionCode);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("authorization/assign-permission", async (
            Request request,
            HttpContext httpContext,
            IAuditTrailService auditTrailService,
            ICommandHandler<AssignPermissionToRoleCommand> handler,
            CancellationToken cancellationToken) =>
        {
            AssignPermissionToRoleCommand command = request.ToCommand();
            Result result = await handler.Handle(command, cancellationToken);

            if (result.IsSuccess)
            {
                string actorId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
                await auditTrailService.RecordAsync(
                    new AuditRecordRequest(
                        actorId,
                        "authorization.assign-permission",
                        "RolePermission",
                        request.RoleName,
                        $"{{\"permissionCode\":\"{request.PermissionCode}\"}}"),
                    cancellationToken);
            }

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.Users)
        .HasPermission(Permissions.AuthorizationManage);
    }
}
