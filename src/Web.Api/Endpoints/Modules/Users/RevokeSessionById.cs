using Application.Abstractions.Messaging;
using Application.Users.Auth;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class RevokeSessionById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/sessions/{sessionId:guid}/revoke", async (
            Guid sessionId,
            ICommandHandler<RevokeSessionByIdCommand> handler,
            CancellationToken cancellationToken) =>
        {
            Result result = await handler.Handle(new RevokeSessionByIdCommand(sessionId), cancellationToken);
            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.Users)
        .RequireAuthorization();
    }
}
