using Application.Abstractions.Messaging;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Profiles;

public sealed record UpdateProfilePreferencesRequest(string? PreferredLanguage, bool ReceiveSecurityAlerts, bool ReceiveProductUpdates);

internal sealed class UpdateMyProfilePreferences : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("profiles/me/preferences", async (
                UpdateProfilePreferencesRequest request,
                ICommandHandler<UpdateMyProfilePreferencesCommand, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(
                new UpdateMyProfilePreferencesCommand(
                    request.PreferredLanguage,
                    request.ReceiveSecurityAlerts,
                    request.ReceiveProductUpdates),
                cancellationToken)).Match(static x => x, CustomResults.Problem))
            .HasPermission(Permissions.ProfilesWrite)
            .WithTags(Tags.Profiles);
    }
}

