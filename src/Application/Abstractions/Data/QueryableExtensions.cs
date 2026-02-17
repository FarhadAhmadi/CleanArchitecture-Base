using System.Linq.Expressions;

namespace Application.Abstractions.Data;

public static class QueryableExtensions
{
    public static (int Page, int PageSize) NormalizePaging(
        int page,
        int pageSize,
        int defaultPageSize = 20,
        int maxPageSize = 200)
    {
        int normalizedPage = page <= 0 ? 1 : page;
        int normalizedPageSize = pageSize <= 0
            ? defaultPageSize
            : Math.Min(pageSize, maxPageSize);

        return (normalizedPage, normalizedPageSize);
    }

    public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, int page, int pageSize)
    {
        (int normalizedPage, int normalizedPageSize) = NormalizePaging(page, pageSize);
        return query.Skip((normalizedPage - 1) * normalizedPageSize).Take(normalizedPageSize);
    }

    public static IQueryable<T> ApplyContainsSearch<T>(
        this IQueryable<T> query,
        string? searchText,
        params Expression<Func<T, string?>>[] selectors)
    {
        if (string.IsNullOrWhiteSpace(searchText) || selectors.Length == 0)
        {
            return query;
        }

        string term = searchText.Trim();
        ParameterExpression parameter = Expression.Parameter(typeof(T), "x");
        Expression? combined = null;

        foreach (Expression<Func<T, string?>> selector in selectors)
        {
            Expression selectorBody = ReplaceParameter(selector, parameter);
            BinaryExpression notNull = Expression.NotEqual(
                selectorBody,
                Expression.Constant(null, typeof(string)));

            MethodCallExpression contains = Expression.Call(
                selectorBody,
                nameof(string.Contains),
                Type.EmptyTypes,
                Expression.Constant(term));

            Expression clause = Expression.AndAlso(notNull, contains);
            combined = combined is null ? clause : Expression.OrElse(combined, clause);
        }

        if (combined is null)
        {
            return query;
        }

        var predicate = Expression.Lambda<Func<T, bool>>(combined, parameter);
        return query.Where(predicate);
    }

    private static Expression ReplaceParameter<T>(Expression<Func<T, string?>> selector, ParameterExpression parameter)
    {
        return new ReplaceParameterVisitor(selector.Parameters[0], parameter).Visit(selector.Body);
    }

    private sealed class ReplaceParameterVisitor(ParameterExpression source, Expression target) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == source ? target : base.VisitParameter(node);
        }
    }
}
