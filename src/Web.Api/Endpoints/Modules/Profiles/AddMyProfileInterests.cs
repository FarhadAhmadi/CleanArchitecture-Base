using Application.Abstractions.Messaging;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Profiles;

internal sealed class AddMyProfileInterests : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("profiles/me/interests", async (
                AddProfileInterestsRequest request,
                ICommandHandler<AddMyProfileInterestsCommand, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new AddMyProfileInterestsCommand(request), cancellationToken)).Match(static x => x, CustomResults.Problem))
            .HasPermission(Permissions.ProfilesWrite)
            .WithTags(Tags.Profiles);
    }
}

