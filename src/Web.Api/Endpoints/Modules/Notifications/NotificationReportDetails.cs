using Microsoft.AspNetCore.Mvc;
using Application.Abstractions.Messaging;
using Application.Notifications;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Notifications;

internal sealed class NotificationReportDetailsEndpoint : IEndpoint, IOrderedEndpoint
{
    public int Order => 9;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("notifications/reports/details", async (
                [AsParameters] NotificationReportDetailsRequest request,
                IQueryHandler<GetNotificationDetailsReportQuery, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new GetNotificationDetailsReportQuery(request), cancellationToken)).Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.NotificationReportsRead)
            .WithTags(Tags.Notifications);
    }
}

