using Application.Abstractions.Messaging;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Profiles;

internal sealed class UpdateMyProfileBasic : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("profiles/me/basic", async (
                UpdateProfileBasicRequest request,
                ICommandHandler<UpdateMyProfileBasicCommand, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new UpdateMyProfileBasicCommand(request), cancellationToken)).Match(static x => x, CustomResults.Problem))
            .HasPermission(Permissions.ProfilesWrite)
            .WithTags(Tags.Profiles);
    }
}

