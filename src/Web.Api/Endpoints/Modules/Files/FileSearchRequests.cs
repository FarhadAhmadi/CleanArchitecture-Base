using Web.Api.Endpoints.Common.Requests;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Files;

public sealed class SearchFilesRequest : PagedQueryRequest
{
    [FromQuery(Name = "query")]
    [SanitizeText(200)]
    public string? Query { get; set; }

    [FromQuery(Name = "fileType")]
    [SanitizeIdentifier(20)]
    public string? FileType { get; set; }

    [FromQuery(Name = "uploaderId")]
    public Guid? UploaderId { get; set; }

    [FromQuery(Name = "from")]
    public DateTime? From { get; set; }

    [FromQuery(Name = "to")]
    public DateTime? To { get; set; }

    protected override int DefaultPageSize => 50;
    protected override int MaxPageSize => 200;
}

public sealed class FilterFilesRequest : PagedQueryRequest
{
    [FromQuery(Name = "module")]
    [SanitizeIdentifier(100)]
    public string? Module { get; set; }

    protected override int DefaultPageSize => 50;
    protected override int MaxPageSize => 200;
}
