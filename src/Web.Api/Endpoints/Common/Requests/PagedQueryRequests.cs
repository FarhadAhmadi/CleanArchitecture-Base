using Application.Abstractions.Data;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Common.Requests;

public class PagedQueryRequest
{
    [FromQuery(Name = "page")]
    public int? Page { get; set; }

    [FromQuery(Name = "pageIndex")]
    public int? PageIndex { get; set; }

    [FromQuery(Name = "pageSize")]
    public int? PageSize { get; set; }

    protected virtual int DefaultPage => 1;

    protected virtual int DefaultPageSize => 20;

    protected virtual int MaxPageSize => 200;

    public (int Page, int PageSize) NormalizePaging()
    {
        int page = PageIndex ?? Page ?? DefaultPage;
        int pageSize = PageSize ?? DefaultPageSize;
        return QueryableExtensions.NormalizePaging(page, pageSize, DefaultPageSize, MaxPageSize);
    }
}

public class PagedSortedQueryRequest : PagedQueryRequest
{
    [FromQuery(Name = "sortBy")]
    [SanitizeIdentifier(50)]
    public string? SortBy { get; set; }

    [FromQuery(Name = "sortOrder")]
    [SanitizeIdentifier(10)]
    public string? SortOrder { get; set; }
}
