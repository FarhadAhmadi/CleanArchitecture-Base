using Application.Abstractions.Messaging;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Profiles;

internal sealed class GetPublicProfile : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("profiles/{userId:guid}/public", async (
                Guid userId,
                IQueryHandler<GetPublicProfileQuery, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new GetPublicProfileQuery(userId), cancellationToken)).Match(static x => x, CustomResults.Problem))
            .HasPermission(Permissions.ProfilesPublicRead)
            .WithTags(Tags.Profiles);
    }
}

