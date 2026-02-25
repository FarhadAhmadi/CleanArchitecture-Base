using Application.Abstractions.Messaging;
using Application.Notifications;
using Domain.Notifications;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Notifications;

public sealed record CreateNotificationTemplateRequest(
    string Name,
    NotificationChannel Channel,
    string Language,
    string SubjectTemplate,
    string BodyTemplate);

internal sealed class NotificationTemplatesEndpoint : IEndpoint, IOrderedEndpoint
{
    public int Order => 4;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("notifications/templates", async (
                CreateNotificationTemplateRequest request,
                ICommandHandler<CreateNotificationTemplateCommand, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(
                new CreateNotificationTemplateCommand(
                    request.Name,
                    request.Channel,
                    request.Language,
                    request.SubjectTemplate,
                    request.BodyTemplate),
                cancellationToken)).Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.NotificationTemplatesManage)
            .WithTags(Tags.Notifications);
    }
}


