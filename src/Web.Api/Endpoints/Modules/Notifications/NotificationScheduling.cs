using Application.Abstractions.Messaging;
using Application.Notifications;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Notifications;

internal sealed class NotificationSchedulingEndpoint : IEndpoint, IOrderedEndpoint
{
    public int Order => 6;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("notifications/{notificationId:guid}/schedule", async (
                Guid notificationId,
                ScheduleNotificationRequest request,
                ICommandHandler<ScheduleNotificationCommand, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new ScheduleNotificationCommand(notificationId, request), cancellationToken)).Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.NotificationSchedulesManage)
            .WithTags(Tags.Notifications);
    }
}


