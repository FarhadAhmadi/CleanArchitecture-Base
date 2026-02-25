using System.Net;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ArchitectureTests.Security;

public sealed class PublicFileLinkSecurityTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public PublicFileLinkSecurityTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task PublicFileLinkEndpoint_ShouldRateLimit_AndWriteAuditLogs()
    {
        using HttpClient client = _factory.CreateClient();
        const string token = "invalid-token";

        HttpResponseMessage first = await client.GetAsync($"/api/v1/files/public/{token}?mode=inline");
        HttpResponseMessage second = await client.GetAsync($"/api/v1/files/public/{token}?mode=inline");
        HttpResponseMessage third = await client.GetAsync($"/api/v1/files/public/{token}?mode=inline");

        first.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        second.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        third.StatusCode.ShouldBe(HttpStatusCode.TooManyRequests);

        using IServiceScope scope = _factory.Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        int auditLogCount = await dbContext.LogEvents
            .CountAsync(x => x.SourceModule == "Files" && x.TagsCsv != null && x.TagsCsv.Contains("public-link"));

        auditLogCount.ShouldBeGreaterThanOrEqualTo(2);
    }
}
