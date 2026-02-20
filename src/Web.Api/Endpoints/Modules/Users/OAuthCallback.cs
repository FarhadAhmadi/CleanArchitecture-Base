using Application.Abstractions.Messaging;
using Application.Users.Auth;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class OAuthCallback : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/oauth/callback", async (
                HttpContext httpContext,
                ICommandHandler<CompleteOAuthCommand, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new CompleteOAuthCommand(httpContext), cancellationToken)).Match(static x => x, CustomResults.Problem))
            .WithTags(Tags.Users);
    }
}



