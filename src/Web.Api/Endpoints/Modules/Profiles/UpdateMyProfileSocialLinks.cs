using Application.Abstractions.Messaging;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Profiles;

public sealed record UpdateProfileSocialLinksRequest(Dictionary<string, string>? Links);

internal sealed class UpdateMyProfileSocialLinks : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("profiles/me/social-links", async (
                UpdateProfileSocialLinksRequest request,
                ICommandHandler<UpdateMyProfileSocialLinksCommand, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new UpdateMyProfileSocialLinksCommand(request.Links), cancellationToken)).Match(static x => x, CustomResults.Problem))
            .HasPermission(Permissions.ProfilesWrite)
            .WithTags(Tags.Profiles);
    }
}

