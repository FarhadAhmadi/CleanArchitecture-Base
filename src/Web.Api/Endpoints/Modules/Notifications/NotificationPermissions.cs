using Application.Abstractions.Messaging;
using Application.Notifications;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Notifications;

internal sealed class NotificationPermissionsEndpoint : IEndpoint, IOrderedEndpoint
{
    public int Order => 7;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("notifications/{notificationId:guid}/permissions", async (
                Guid notificationId,
                UpsertNotificationPermissionRequest request,
                ICommandHandler<UpsertNotificationPermissionCommand, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new UpsertNotificationPermissionCommand(notificationId, request), cancellationToken)).Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.NotificationPermissionsManage)
            .WithTags(Tags.Notifications);
    }
}


