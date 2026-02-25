using Application.Abstractions.Messaging;
using Application.Users.Auth;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class GetSessions : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/sessions", async (
            IQueryHandler<GetUserSessionsQuery, List<UserSessionResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            Result<List<UserSessionResponse>> result =
                await handler.Handle(new GetUserSessionsQuery(), cancellationToken);
            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Users)
        .RequireAuthorization();
    }
}
