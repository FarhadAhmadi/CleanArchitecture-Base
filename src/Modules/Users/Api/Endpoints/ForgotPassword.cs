using Application.Abstractions.Messaging;
using Application.Users.Auth;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class ForgotPassword : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/forgot-password", async (
                ForgotPasswordRequest request,
                ICommandHandler<RequestPasswordResetCommand, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new RequestPasswordResetCommand(request.Email), cancellationToken)).Match(static x => x, CustomResults.Problem))
            .WithTags(Tags.Users);
    }
}

