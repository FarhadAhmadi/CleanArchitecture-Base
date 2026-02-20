using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Application.Users.GetById;
using SharedKernel;
using Web.Api.Endpoints.Mappings;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class GetMe : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/me", async (
            IUserContext userContext,
            IQueryHandler<GetUserByIdQuery, UserResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var query = userContext.UserId.ToGetUserByIdQuery();
            Result<UserResponse> result = await handler.Handle(query, cancellationToken);
            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Users)
        .RequireAuthorization();
    }
}
