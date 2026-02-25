using Application.Abstractions.Messaging;
using Application.Users.Auth;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class ResetPassword : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/reset-password", async (
                ResetPasswordRequest request,
                ICommandHandler<ConfirmPasswordResetCommand, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new ConfirmPasswordResetCommand(request.Email, request.Code, request.NewPassword), cancellationToken)).Match(static x => x, CustomResults.Problem))
            .WithTags(Tags.Users);
    }
}

