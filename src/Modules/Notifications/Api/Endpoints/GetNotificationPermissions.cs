using Application.Abstractions.Messaging;
using Application.Notifications;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Notifications;

internal sealed class GetNotificationPermissions : IEndpoint, IOrderedEndpoint
{
    public int Order => 7;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("notifications/{notificationId:guid}/permissions", async (
                Guid notificationId,
                IQueryHandler<GetNotificationPermissionsQuery, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new GetNotificationPermissionsQuery(notificationId), cancellationToken)).Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.NotificationPermissionsManage)
            .WithTags(Tags.Notifications);
    }
}


