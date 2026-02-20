using Microsoft.AspNetCore.Mvc;
using Application.Abstractions.Messaging;
using Application.Notifications;
using Domain.Notifications;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Notifications;

public sealed class NotificationReportDetailsRequest
{
    public int? Page { get; set; }
    public int? PageIndex { get; set; }
    public int? PageSize { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public NotificationChannel? Channel { get; set; }
    public NotificationStatus? Status { get; set; }
}

internal sealed class NotificationReportDetailsEndpoint : IEndpoint, IOrderedEndpoint
{
    public int Order => 9;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("notifications/reports/details", async (
                [AsParameters] NotificationReportDetailsRequest request,
                IQueryHandler<GetNotificationDetailsReportQuery, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(
                new GetNotificationDetailsReportQuery(
                    request.Page,
                    request.PageIndex,
                    request.PageSize,
                    request.From,
                    request.To,
                    request.Channel,
                    request.Status),
                cancellationToken)).Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.NotificationReportsRead)
            .WithTags(Tags.Notifications);
    }
}

