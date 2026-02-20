using Application.Abstractions.Messaging;
using Application.Notifications;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Notifications;

internal sealed class ArchiveNotification : IEndpoint, IOrderedEndpoint
{
    public int Order => 7;
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("notifications/archive/{id:guid}", async (
                Guid id,
                ICommandHandler<ArchiveNotificationCommand, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new ArchiveNotificationCommand(id), cancellationToken)).Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.NotificationsWrite)
            .WithTags(Tags.Notifications);
    }
}




