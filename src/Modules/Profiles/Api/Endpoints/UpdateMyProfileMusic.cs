using Application.Abstractions.Messaging;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Profiles;

public sealed record UpdateProfileMusicRequest(string? MusicTitle, string? MusicArtist, Guid? MusicFileId);

internal sealed class UpdateMyProfileMusic : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("profiles/me/music", async (
                UpdateProfileMusicRequest request,
                ICommandHandler<UpdateMyProfileMusicCommand, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(
                new UpdateMyProfileMusicCommand(request.MusicTitle, request.MusicArtist, request.MusicFileId),
                cancellationToken)).Match(static x => x, CustomResults.Problem))
            .HasPermission(Permissions.ProfilesWrite)
            .WithTags(Tags.Profiles);
    }
}

