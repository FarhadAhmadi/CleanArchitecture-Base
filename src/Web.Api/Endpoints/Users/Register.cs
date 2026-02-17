using Application.Abstractions.Messaging;
using Application.Users.Register;
using Infrastructure.Auditing;
using SharedKernel;
using Web.Api.Endpoints.Mappings;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class Register : IEndpoint
{
    public sealed record Request(string Email, string FirstName, string LastName, string Password);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/register", async (
            Request request,
            IAuditTrailService auditTrailService,
            ICommandHandler<RegisterUserCommand, Guid> handler,
            CancellationToken cancellationToken) =>
        {
            RegisterUserCommand command = request.ToCommand();

            Result<Guid> result = await handler.Handle(command, cancellationToken);

            await auditTrailService.RecordAsync(
                new AuditRecordRequest(
                    request.Email,
                    result.IsSuccess ? "auth.register.success" : "auth.register.failed",
                    "User",
                    result.IsSuccess ? result.Value.ToString("N") : request.Email,
                    "{\"scope\":\"user-register\"}"),
                cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Users);
    }
}
