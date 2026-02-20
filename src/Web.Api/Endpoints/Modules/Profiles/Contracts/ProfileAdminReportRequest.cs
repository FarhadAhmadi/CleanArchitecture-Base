using Microsoft.AspNetCore.Mvc;
using Web.Api.Endpoints.Common.Requests;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Profiles;

public sealed class ProfileAdminReportRequest : PagedQueryRequest
{
    [FromQuery(Name = "search")]
    [SanitizeText(100)]
    public string? Search { get; set; }

    [FromQuery(Name = "isPublic")]
    public bool? IsProfilePublic { get; set; }

    [FromQuery(Name = "language")]
    [SanitizeIdentifier(16)]
    public string? PreferredLanguage { get; set; }

    [FromQuery(Name = "minCompletion")]
    public int? MinCompleteness { get; set; }

    [FromQuery(Name = "maxCompletion")]
    public int? MaxCompleteness { get; set; }

    [FromQuery(Name = "updatedFrom")]
    public DateTime? UpdatedFrom { get; set; }

    [FromQuery(Name = "updatedTo")]
    public DateTime? UpdatedTo { get; set; }

    protected override int DefaultPageSize => 50;
    protected override int MaxPageSize => 200;
}
