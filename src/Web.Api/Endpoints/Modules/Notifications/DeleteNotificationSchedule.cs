using Application.Abstractions.Messaging;
using Application.Notifications;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Notifications;

internal sealed class DeleteNotificationSchedule : IEndpoint, IOrderedEndpoint
{
    public int Order => 7;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("notifications/schedules/{scheduleId:guid}", async (
                Guid scheduleId,
                ICommandHandler<DeleteNotificationScheduleCommand, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new DeleteNotificationScheduleCommand(scheduleId), cancellationToken)).Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.NotificationSchedulesManage)
            .WithTags(Tags.Notifications);
    }
}


