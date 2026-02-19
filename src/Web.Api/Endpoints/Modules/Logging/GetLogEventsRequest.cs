using Domain.Logging;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Endpoints.Common.Requests;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Logging;

public sealed class GetLogEventsRequest : PagedSortedQueryRequest
{
    [FromQuery(Name = "level")]
    public LogLevelType? Level { get; set; }

    [FromQuery(Name = "from")]
    public DateTime? From { get; set; }

    [FromQuery(Name = "to")]
    public DateTime? To { get; set; }

    [FromQuery(Name = "actorId")]
    [SanitizeIdentifier(100)]
    public string? ActorId { get; set; }

    [FromQuery(Name = "service")]
    [SanitizeIdentifier(150)]
    public string? Service { get; set; }

    [FromQuery(Name = "module")]
    [SanitizeIdentifier(150)]
    public string? Module { get; set; }

    [FromQuery(Name = "traceId")]
    [SanitizeIdentifier(150)]
    public string? TraceId { get; set; }

    [FromQuery(Name = "outcome")]
    [SanitizeIdentifier(50)]
    public string? Outcome { get; set; }

    [FromQuery(Name = "text")]
    [SanitizeText(500)]
    public string? Text { get; set; }

    [FromQuery(Name = "recalculateIntegrity")]
    public bool RecalculateIntegrity { get; set; }

    protected override int DefaultPageSize => 50;
    protected override int MaxPageSize => 200;
}
