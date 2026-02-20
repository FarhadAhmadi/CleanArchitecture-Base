using Application.Abstractions.Messaging;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Profiles;

internal sealed class DeleteMyProfileAvatar : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("profiles/me/avatar", async (
                ICommandHandler<DeleteMyProfileAvatarCommand, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new DeleteMyProfileAvatarCommand(), cancellationToken)).Match(static x => x, CustomResults.Problem))
            .HasPermission(Permissions.ProfilesWrite)
            .WithTags(Tags.Profiles);
    }
}

