using Application.Abstractions.Messaging;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Profiles;

internal sealed class GetMyProfile : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("profiles/me", async (
                IQueryHandler<GetMyProfileQuery, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new GetMyProfileQuery(), cancellationToken)).Match(static x => x, CustomResults.Problem))
            .HasPermission(Permissions.ProfilesRead)
            .WithTags(Tags.Profiles);
    }
}

