using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Application.Users.Tokens;
using SharedKernel;
using Web.Api.Endpoints.Mappings;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class Refresh : IEndpoint
{
    public sealed record Request(string RefreshToken);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/refresh", async (
            Request request,
            ICommandHandler<RefreshTokenCommand, TokenResponse> handler,
            CancellationToken cancellationToken) =>
        {
            RefreshTokenCommand command = request.ToCommand();

            Result<TokenResponse> result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Users);
    }
}
