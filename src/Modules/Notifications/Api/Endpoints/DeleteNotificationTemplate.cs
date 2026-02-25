using Application.Abstractions.Messaging;
using Application.Notifications;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Notifications;

internal sealed class DeleteNotificationTemplate : IEndpoint, IOrderedEndpoint
{
    public int Order => 12;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("notification-templates/{templateId:guid}", async (
                Guid templateId,
                ICommandHandler<DeleteNotificationTemplateCommand, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new DeleteNotificationTemplateCommand(templateId), cancellationToken)).Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.NotificationTemplatesManage)
            .WithTags(Tags.Notifications);
    }
}


