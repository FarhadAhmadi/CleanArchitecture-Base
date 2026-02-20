using Application.Abstractions.Messaging;
using Application.Notifications;
using Domain.Notifications;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Notifications;

public sealed record CreateNotificationRequest(
    NotificationChannel Channel,
    NotificationPriority Priority,
    string Recipient,
    string? Subject,
    string? Body,
    string Language,
    Guid? TemplateId,
    DateTime? ScheduledAtUtc);

internal sealed class CreateNotification : IEndpoint, IOrderedEndpoint
{
    public int Order => 1;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("notifications", async (
                CreateNotificationRequest request,
                ICommandHandler<CreateNotificationCommand, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(
                new CreateNotificationCommand(
                    request.Channel,
                    request.Priority,
                    request.Recipient,
                    request.Subject,
                    request.Body,
                    request.Language,
                    request.TemplateId,
                    request.ScheduledAtUtc),
                cancellationToken)).Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.NotificationsWrite)
            .WithTags(Tags.Notifications);
    }
}


