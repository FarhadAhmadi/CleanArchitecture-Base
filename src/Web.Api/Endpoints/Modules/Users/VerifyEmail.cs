using Application.Abstractions.Messaging;
using Application.Users.Auth;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class VerifyEmail : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/verify-email", async (
            VerifyEmailRequest request,
            ICommandHandler<VerifyEmailCommand, IResult> handler,
            CancellationToken cancellationToken) =>
            (await handler.Handle(new VerifyEmailCommand(request.Email, request.Code), cancellationToken)).Match(static x => x, CustomResults.Problem))
        .WithTags(Tags.Users);
    }
}

