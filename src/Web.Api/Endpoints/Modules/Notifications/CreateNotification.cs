using Application.Abstractions.Messaging;
using Application.Notifications;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Notifications;

internal sealed class CreateNotification : IEndpoint, IOrderedEndpoint
{
    public int Order => 1;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("notifications", async (
                CreateNotificationRequest request,
                ICommandHandler<CreateNotificationCommand, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new CreateNotificationCommand(request), cancellationToken)).Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.NotificationsWrite)
            .WithTags(Tags.Notifications);
    }
}


