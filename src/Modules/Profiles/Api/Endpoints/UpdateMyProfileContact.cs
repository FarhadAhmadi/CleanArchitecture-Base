using Application.Abstractions.Messaging;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Profiles;

public sealed record UpdateProfileContactRequest(string? ContactEmail, string? ContactPhone, string? Website, string? TimeZone);

internal sealed class UpdateMyProfileContact : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("profiles/me/contact", async (
                UpdateProfileContactRequest request,
                ICommandHandler<UpdateMyProfileContactCommand, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(
                new UpdateMyProfileContactCommand(request.ContactEmail, request.ContactPhone, request.Website, request.TimeZone),
                cancellationToken)).Match(static x => x, CustomResults.Problem))
            .HasPermission(Permissions.ProfilesWrite)
            .WithTags(Tags.Profiles);
    }
}

