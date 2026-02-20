using Application.Abstractions.Data;

namespace Application.Audit;

public sealed class GetAuditEntriesRequest
{
    public int? Page { get; set; }
    public int? PageIndex { get; set; }
    public int? PageSize { get; set; }
    public string? ActorId { get; set; }
    public string? Action { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }

    public (int Page, int PageSize) NormalizePaging()
    {
        int page = PageIndex ?? Page ?? 1;
        int pageSize = PageSize ?? 50;
        return QueryableExtensions.NormalizePaging(page, pageSize, 50, 200);
    }
}


