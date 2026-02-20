using Application.Abstractions.Messaging;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Profiles;

public sealed record CreateProfileRequest(string? DisplayName, string? PreferredLanguage, bool IsProfilePublic);

internal sealed class CreateMyProfile : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("profiles/me", async (
                CreateProfileRequest request,
                ICommandHandler<CreateMyProfileCommand, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(
                new CreateMyProfileCommand(request.DisplayName, request.PreferredLanguage, request.IsProfilePublic),
                cancellationToken)).Match(static x => x, CustomResults.Problem))
            .HasPermission(Permissions.ProfilesWrite)
            .WithTags(Tags.Profiles);
    }
}

