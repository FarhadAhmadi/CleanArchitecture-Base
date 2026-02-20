using Application.Abstractions.Messaging;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Profiles;

internal sealed class UpdateMyProfileBio : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("profiles/me/bio", async (
                UpdateProfileBioRequest request,
                ICommandHandler<UpdateMyProfileBioCommand, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new UpdateMyProfileBioCommand(request), cancellationToken)).Match(static x => x, CustomResults.Problem))
            .HasPermission(Permissions.ProfilesWrite)
            .WithTags(Tags.Profiles);
    }
}

