using Application.Abstractions.Data;
using Shouldly;

namespace ArchitectureTests.Modules.Audit;

public sealed class AuditPagingUnitTests
{
    [Fact]
    public void NormalizePaging_ShouldApplyBounds()
    {
        (int page, int pageSize) = QueryableExtensions.NormalizePaging(page: 0, pageSize: 999, defaultPageSize: 50, maxPageSize: 200);

        page.ShouldBe(1);
        pageSize.ShouldBe(200);
    }

    [Fact]
    public void ApplyPaging_ShouldReturnExpectedSlice()
    {
        var data = Enumerable.Range(1, 30).ToList();

        var slice = data.AsQueryable().ApplyPaging(page: 2, pageSize: 10).ToList();

        slice.ShouldBe([11, 12, 13, 14, 15, 16, 17, 18, 19, 20]);
    }
}
