using Application.Abstractions.Messaging;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Profiles;

internal sealed class RemoveMyProfileInterest : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("profiles/me/interests/{interest}", async (
                string interest,
                ICommandHandler<RemoveMyProfileInterestCommand, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new RemoveMyProfileInterestCommand(interest), cancellationToken)).Match(static x => x, CustomResults.Problem))
            .HasPermission(Permissions.ProfilesWrite)
            .WithTags(Tags.Profiles);
    }
}

