using Microsoft.AspNetCore.Mvc;
using Application.Abstractions.Messaging;
using Application.Notifications;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Notifications;

internal sealed class ListNotificationsEndpoint : IEndpoint, IOrderedEndpoint
{
    public int Order => 3;
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("notifications", async (
                [AsParameters] ListNotificationsRequest request,
                IQueryHandler<ListNotificationsQuery, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new ListNotificationsQuery(request), cancellationToken)).Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.NotificationsRead)
            .WithTags(Tags.Notifications);
    }
}

