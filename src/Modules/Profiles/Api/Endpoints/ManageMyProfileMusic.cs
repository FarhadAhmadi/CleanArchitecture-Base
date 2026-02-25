using Application.Abstractions.Messaging;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Profiles;

internal sealed class ManageMyProfileMusic : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("profiles/me/music", async (
                IQueryHandler<GetMyProfileMusicQuery, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new GetMyProfileMusicQuery(), cancellationToken)).Match(static x => x, CustomResults.Problem))
            .HasPermission(Permissions.ProfilesRead)
            .WithTags(Tags.Profiles);
    }
}

