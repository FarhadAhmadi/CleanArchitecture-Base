using Application.Abstractions.Messaging;
using Application.Notifications;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Notifications;

internal sealed class GetNotificationTemplateEndpoint : IEndpoint, IOrderedEndpoint
{
    public int Order => 10;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("notification-templates/{templateId:guid}", async (
                Guid templateId,
                IQueryHandler<GetNotificationTemplateQuery, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new GetNotificationTemplateQuery(templateId), cancellationToken)).Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.NotificationTemplatesManage)
            .WithTags(Tags.Notifications);
    }
}

