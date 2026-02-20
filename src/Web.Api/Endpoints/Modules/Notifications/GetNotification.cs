using Application.Abstractions.Messaging;
using Application.Notifications;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Notifications;

internal sealed class GetNotification : IEndpoint, IOrderedEndpoint
{
    public int Order => 2;
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("notifications/{notificationId:guid}", async (
                Guid notificationId,
                IQueryHandler<GetNotificationQuery, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new GetNotificationQuery(notificationId), cancellationToken)).Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.NotificationsRead)
            .WithTags(Tags.Notifications);
    }
}




