using Application.Abstractions.Messaging;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Profiles;

public sealed record UpdateProfilePrivacyRequest(bool IsProfilePublic, bool ShowEmail, bool ShowPhone);

internal sealed class UpdateMyProfilePrivacy : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("profiles/me/privacy", async (
                UpdateProfilePrivacyRequest request,
                ICommandHandler<UpdateMyProfilePrivacyCommand, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(
                new UpdateMyProfilePrivacyCommand(request.IsProfilePublic, request.ShowEmail, request.ShowPhone),
                cancellationToken)).Match(static x => x, CustomResults.Problem))
            .HasPermission(Permissions.ProfilesWrite)
            .WithTags(Tags.Profiles);
    }
}

