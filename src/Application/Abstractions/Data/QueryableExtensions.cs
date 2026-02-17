namespace Application.Abstractions.Data;

public static class QueryableExtensions
{
    public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, int page, int pageSize)
    {
        int normalizedPage = page <= 0 ? 1 : page;
        int normalizedPageSize = pageSize <= 0 ? 20 : pageSize;
        return query.Skip((normalizedPage - 1) * normalizedPageSize).Take(normalizedPageSize);
    }
}
