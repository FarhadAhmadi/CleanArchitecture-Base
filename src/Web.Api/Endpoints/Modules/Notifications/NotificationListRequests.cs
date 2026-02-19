using Domain.Notifications;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Endpoints.Common.Requests;

namespace Web.Api.Endpoints.Notifications;

public sealed class ListNotificationsRequest : PagedQueryRequest
{
    [FromQuery(Name = "channel")]
    public NotificationChannel? Channel { get; set; }

    [FromQuery(Name = "status")]
    public NotificationStatus? Status { get; set; }

    [FromQuery(Name = "from")]
    public DateTime? From { get; set; }

    [FromQuery(Name = "to")]
    public DateTime? To { get; set; }

    protected override int DefaultPageSize => 50;
    protected override int MaxPageSize => 200;
}

public sealed class NotificationReportDetailsRequest : PagedQueryRequest
{
    [FromQuery(Name = "from")]
    public DateTime? From { get; set; }

    [FromQuery(Name = "to")]
    public DateTime? To { get; set; }

    [FromQuery(Name = "channel")]
    public NotificationChannel? Channel { get; set; }

    [FromQuery(Name = "status")]
    public NotificationStatus? Status { get; set; }

    protected override int DefaultPageSize => 50;
    protected override int MaxPageSize => 200;
}
