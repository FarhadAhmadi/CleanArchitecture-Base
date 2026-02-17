using Application.Abstractions.Messaging;
using Application.Authorization.AssignRoleToUser;
using Infrastructure.Auditing;
using SharedKernel;
using System.Security.Claims;
using Web.Api.Endpoints.Mappings;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Authorization;

internal sealed class AssignRoleToUser : IEndpoint
{
    public sealed record Request(Guid UserId, string RoleName);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("authorization/assign-role", async (
            Request request,
            HttpContext httpContext,
            IAuditTrailService auditTrailService,
            ICommandHandler<AssignRoleToUserCommand> handler,
            CancellationToken cancellationToken) =>
        {
            AssignRoleToUserCommand command = request.ToCommand();
            Result result = await handler.Handle(command, cancellationToken);

            if (result.IsSuccess)
            {
                string actorId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
                await auditTrailService.RecordAsync(
                    new AuditRecordRequest(
                        actorId,
                        "authorization.assign-role",
                        "UserRole",
                        request.UserId.ToString("N"),
                        $"{{\"roleName\":\"{request.RoleName}\"}}"),
                    cancellationToken);
            }

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.Users)
        .HasPermission(Permissions.AuthorizationManage);
    }
}
