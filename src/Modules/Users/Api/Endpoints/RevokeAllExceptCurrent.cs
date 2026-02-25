using Application.Abstractions.Messaging;
using Application.Users.Auth;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class RevokeAllExceptCurrent : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/sessions/revoke-all-except-current", async (
            ICommandHandler<RevokeAllExceptCurrentCommand, IResult> handler,
            CancellationToken cancellationToken) =>
            (await handler.Handle(new RevokeAllExceptCurrentCommand(), cancellationToken))
            .Match(static x => x, CustomResults.Problem))
        .WithTags(Tags.Users)
        .RequireAuthorization();
    }
}
