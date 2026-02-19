using Web.Api.Endpoints.Common.Requests;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Audit;

public sealed class GetAuditEntriesRequest : PagedQueryRequest
{
    [FromQuery(Name = "actorId")]
    [SanitizeIdentifier(100)]
    public string? ActorId { get; set; }

    [FromQuery(Name = "action")]
    [SanitizeIdentifier(150)]
    public string? Action { get; set; }

    [FromQuery(Name = "from")]
    public DateTime? From { get; set; }

    [FromQuery(Name = "to")]
    public DateTime? To { get; set; }

    protected override int DefaultPageSize => 50;
    protected override int MaxPageSize => 200;
}
