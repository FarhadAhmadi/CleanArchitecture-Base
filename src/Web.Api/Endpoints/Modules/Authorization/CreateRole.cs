using System.Security.Claims;
using Application.Abstractions.Messaging;
using Application.Authorization.Roles;
using Infrastructure.Auditing;
using SharedKernel;
using Web.Api.Endpoints.Mappings;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Authorization;

internal sealed class CreateRole : IEndpoint
{
    public sealed record Request(string RoleName);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("authorization/roles", async (
            Request request,
            HttpContext httpContext,
            IAuditTrailService auditTrailService,
            ICommandHandler<CreateRoleCommand, RoleCrudResponse> handler,
            CancellationToken cancellationToken) =>
        {
            CreateRoleCommand command = request.ToCommand();
            Result<RoleCrudResponse> result = await handler.Handle(command, cancellationToken);

            if (result.IsSuccess)
            {
                string actorId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
                await auditTrailService.RecordAsync(
                    new AuditRecordRequest(
                        actorId,
                        "authorization.role.create",
                        "Role",
                        result.Value.Id.ToString("N"),
                        $"{{\"roleName\":\"{result.Value.Name}\"}}"),
                    cancellationToken);
            }

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Users)
        .HasPermission(Permissions.AuthorizationManage);
    }
}
