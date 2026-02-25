using Application.Abstractions.Messaging;
using Application.Notifications;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Notifications;

internal sealed class ListNotificationSchedulesEndpoint : IEndpoint, IOrderedEndpoint
{
    public int Order => 6;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("notifications/schedules", async (
                IQueryHandler<ListNotificationSchedulesQuery, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new ListNotificationSchedulesQuery(), cancellationToken)).Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.NotificationSchedulesManage)
            .WithTags(Tags.Notifications);
    }
}

