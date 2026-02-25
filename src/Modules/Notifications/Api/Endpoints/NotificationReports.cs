using Application.Abstractions.Messaging;
using Application.Notifications;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Notifications;

internal sealed class NotificationReportsEndpoint : IEndpoint, IOrderedEndpoint
{
    public int Order => 8;
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("notifications/reports/summary", async (
                DateTime? from,
                DateTime? to,
                IQueryHandler<GetNotificationSummaryReportQuery, IResult> handler,
                CancellationToken cancellationToken) =>
            (await handler.Handle(new GetNotificationSummaryReportQuery(from, to), cancellationToken)).Match(static x => x, Web.Api.Infrastructure.CustomResults.Problem))
            .HasPermission(Permissions.NotificationReportsRead)
            .WithTags(Tags.Notifications);
    }
}



