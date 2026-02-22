using System.Security.Claims;
using Application.Abstractions.Messaging;
using Application.Users.Management;
using Infrastructure.Auditing;
using SharedKernel;
using Web.Api.Endpoints.Mappings;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class UpdateUser : IEndpoint
{
    public sealed record Request(string Email, string FirstName, string LastName);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("users/{userId:guid}", async (
            Guid userId,
            Request request,
            HttpContext httpContext,
            IAuditTrailService auditTrailService,
            ICommandHandler<UpdateUserCommand, UserAdminResponse> handler,
            CancellationToken cancellationToken) =>
        {
            UpdateUserCommand command = request.ToCommand(userId);
            Result<UserAdminResponse> result = await handler.Handle(command, cancellationToken);

            if (result.IsSuccess)
            {
                string actorId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
                await auditTrailService.RecordAsync(
                    new AuditRecordRequest(
                        actorId,
                        "users.update",
                        "User",
                        result.Value.Id.ToString("N"),
                        $"{{\"email\":\"{result.Value.Email}\"}}"),
                    cancellationToken);
            }

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Users)
        .HasPermission(Permissions.UsersAccess);
    }
}
