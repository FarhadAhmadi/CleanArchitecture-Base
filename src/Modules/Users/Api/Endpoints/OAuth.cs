using Application.Abstractions.Messaging;
using Application.Users.Auth;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class OAuth : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/oauth/{provider}/start", async (
                string provider,
                string? returnUrl,
                IQueryHandler<StartOAuthQuery, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new StartOAuthQuery(provider, returnUrl), cancellationToken)).Match(static x => x, CustomResults.Problem))
            .WithTags(Tags.Users);
    }
}



