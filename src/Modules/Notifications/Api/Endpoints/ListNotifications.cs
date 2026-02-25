using Microsoft.AspNetCore.Mvc;
using Application.Abstractions.Messaging;
using Application.Notifications;
using Domain.Notifications;
using Web.Api.Endpoints.Common.Requests;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Notifications;

public sealed class ListNotificationsRequest : PagedSortedQueryRequest
{
    public NotificationChannel? Channel { get; set; }
    public NotificationStatus? Status { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}

internal sealed class ListNotificationsEndpoint : IEndpoint, IOrderedEndpoint
{
    public int Order => 3;
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("notifications", async (
                [AsParameters] ListNotificationsRequest request,
                IQueryHandler<ListNotificationsQuery, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(
                new ListNotificationsQuery(
                    request.Page,
                    request.PageIndex,
                    request.PageSize,
                    request.Channel,
                    request.Status,
                    request.From,
                    request.To),
                cancellationToken)).Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.NotificationsRead)
            .WithTags(Tags.Notifications);
    }
}

