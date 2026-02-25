using Domain.Notifications;
using Application.Abstractions.Messaging;
using Application.Notifications;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Notifications;

internal sealed class ListNotificationTemplatesEndpoint : IEndpoint, IOrderedEndpoint
{
    public int Order => 13;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("notification-templates", async (
                string? language,
                NotificationChannel? channel,
                IQueryHandler<ListNotificationTemplatesQuery, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new ListNotificationTemplatesQuery(language, channel), cancellationToken)).Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.NotificationTemplatesManage)
            .WithTags(Tags.Notifications);
    }
}

