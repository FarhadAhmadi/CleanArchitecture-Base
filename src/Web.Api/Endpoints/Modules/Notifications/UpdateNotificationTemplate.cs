using Application.Abstractions.Messaging;
using Application.Notifications;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Notifications;

public sealed record UpdateNotificationTemplateRequest(string SubjectTemplate, string BodyTemplate);

internal sealed class UpdateNotificationTemplateEndpoint : IEndpoint, IOrderedEndpoint
{
    public int Order => 5;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("notifications/templates/{templateId:guid}", async (
                Guid templateId,
                UpdateNotificationTemplateRequest request,
                ICommandHandler<UpdateNotificationTemplateCommand, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(
                new UpdateNotificationTemplateCommand(templateId, request.SubjectTemplate, request.BodyTemplate),
                cancellationToken)).Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.NotificationTemplatesManage)
            .WithTags(Tags.Notifications);
    }
}


