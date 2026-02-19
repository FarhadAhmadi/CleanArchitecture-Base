using Web.Api.Endpoints.Common.Requests;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Todos;

public sealed class GetTodosRequest : PagedSortedQueryRequest
{
    [FromQuery(Name = "search")]
    [SanitizeText(200)]
    public string? Search { get; set; }

    [FromQuery(Name = "isCompleted")]
    public bool? IsCompleted { get; set; }

    protected override int DefaultPageSize => 15;
    protected override int MaxPageSize => 100;
}
