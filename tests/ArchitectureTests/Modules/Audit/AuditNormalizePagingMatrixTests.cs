using Application.Abstractions.Data;
using Shouldly;

namespace ArchitectureTests.Modules.Audit;

public sealed class AuditNormalizePagingMatrixTests
{
    [Theory]
    [MemberData(nameof(PagingCases))]
    public void NormalizePaging_ShouldHandleMatrixOfInputs(int page, int pageSize)
    {
        (int normalizedPage, int normalizedPageSize) = QueryableExtensions.NormalizePaging(
            page,
            pageSize,
            defaultPageSize: 50,
            maxPageSize: 200);

        int expectedPage = page <= 0 ? 1 : page;
        int expectedPageSize = pageSize <= 0 ? 50 : Math.Min(pageSize, 200);

        normalizedPage.ShouldBe(expectedPage);
        normalizedPageSize.ShouldBe(expectedPageSize);
    }

    public static IEnumerable<object[]> PagingCases()
    {
        int[] pages = [-5, -1, 0, 1, 2, 3, 10, 50, 999, -999];
        int[] sizes = [-20, -1, 0, 1, 2, 20, 50, 200, 201, 1000];

        foreach (int page in pages)
        {
            foreach (int pageSize in sizes)
            {
                yield return [page, pageSize];
            }
        }
    }
}
