using System.Net;
using ArchitectureTests.Security;
using Shouldly;

namespace ArchitectureTests.Modules.Audit;

public sealed class AuditIntegrationTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public AuditIntegrationTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task AuditEndpoints_WithoutToken_ShouldReturnUnauthorized()
    {
        using HttpClient client = _factory.CreateClient();

        HttpResponseMessage trail = await client.GetAsync("/api/v1/audit?page=1&pageSize=10");
        HttpResponseMessage integrity = await client.GetAsync("/api/v1/audit/integrity?updateFlags=false");

        trail.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        integrity.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AuditEndpoints_WithUserRole_ShouldReturnForbidden()
    {
        using HttpClient client = _factory.CreateClient();
        await TestIdentityHelper.RegisterLoginAndAssignRoleAsync(_factory, client, "user");

        HttpResponseMessage trail = await client.GetAsync("/api/v1/audit?page=1&pageSize=10");
        HttpResponseMessage integrity = await client.GetAsync("/api/v1/audit/integrity?updateFlags=false");

        trail.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
        integrity.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task AuditEndpoints_WithAdminRole_ShouldReturnOk()
    {
        using HttpClient client = _factory.CreateClient();
        await TestIdentityHelper.RegisterLoginAndAssignRoleAsync(_factory, client, "admin");

        HttpResponseMessage trail = await client.GetAsync("/api/v1/audit?page=1&pageSize=10");
        HttpResponseMessage integrity = await client.GetAsync("/api/v1/audit/integrity?updateFlags=false");

        trail.StatusCode.ShouldBe(HttpStatusCode.OK);
        integrity.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
