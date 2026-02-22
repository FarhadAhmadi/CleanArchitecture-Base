using System.Security.Claims;
using Application.Abstractions.Messaging;
using Application.Users.Management;
using Infrastructure.Auditing;
using SharedKernel;
using Web.Api.Endpoints.Mappings;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class CreateUser : IEndpoint
{
    public sealed record Request(string Email, string FirstName, string LastName, string Password);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users", async (
            Request request,
            HttpContext httpContext,
            IAuditTrailService auditTrailService,
            ICommandHandler<CreateUserCommand, UserAdminResponse> handler,
            CancellationToken cancellationToken) =>
        {
            CreateUserCommand command = request.ToCommand();
            Result<UserAdminResponse> result = await handler.Handle(command, cancellationToken);

            if (result.IsSuccess)
            {
                string actorId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "system";
                await auditTrailService.RecordAsync(
                    new AuditRecordRequest(
                        actorId,
                        "users.create",
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
