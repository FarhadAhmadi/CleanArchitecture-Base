using Application.Abstractions.Messaging;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Profiles;

public sealed record UpdateProfileAvatarRequest(Guid? AvatarFileId);

internal sealed class UpdateMyProfileAvatar : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("profiles/me/avatar", async (
                UpdateProfileAvatarRequest request,
                ICommandHandler<UpdateMyProfileAvatarCommand, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new UpdateMyProfileAvatarCommand(request.AvatarFileId), cancellationToken)).Match(static x => x, CustomResults.Problem))
            .HasPermission(Permissions.ProfilesWrite)
            .WithTags(Tags.Profiles);
    }
}

