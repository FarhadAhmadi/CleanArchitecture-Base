using Application.Abstractions.Messaging;
using Application.Abstractions.Users;
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
        app.MapPost("users/register", async Task<IResult> (
            Request request,
            HttpContext httpContext,
            IUserRegistrationVerificationService verificationService,
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

            if (!result.IsSuccess)
            {
                return CustomResults.Problem(result);
            }

            DateTime verificationExpiresAtUtc = verificationService.GetVerificationExpiryUtc(DateTime.UtcNow);
            httpContext.Response.Headers["X-Verification-Required"] = "true";
            httpContext.Response.Headers["X-Verification-Expires-At-Utc"] = verificationExpiresAtUtc.ToString("O");

            return Results.Ok(result.Value);
        })
        .WithTags(Tags.Users);
    }
}
