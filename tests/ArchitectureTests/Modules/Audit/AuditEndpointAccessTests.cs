using System.Net;
using ArchitectureTests.Modules.Common;
using ArchitectureTests.Security;
using Shouldly;

namespace ArchitectureTests.Modules.Audit;

[Collection(ArchitectureTests.Modules.Common.EndpointAccessCollection.Name)]
public sealed class AuditEndpointAccessTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public AuditEndpointAccessTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    public static IEnumerable<object[]> Endpoints()
    {
        yield return [new EndpointAccessCase("GetAuditTrail", HttpMethod.Get, "audit?page=1&pageSize=10")];
        yield return [new EndpointAccessCase("GetAuditIntegrityReport", HttpMethod.Get, "audit/integrity?updateFlags=false")];
    }

    [Theory]
    [MemberData(nameof(Endpoints))]
    public async Task Endpoint_WithoutToken_ShouldReturnUnauthorized(EndpointAccessCase testCase)
    {
        using HttpClient anonymousClient = _factory.CreateClient();

        HttpResponseMessage response = await testCase.SendAsync(anonymousClient);

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized, testCase.Name);
    }

    [Theory]
    [MemberData(nameof(Endpoints))]
    public async Task Endpoint_WithUserRole_ShouldReturnForbidden(EndpointAccessCase testCase)
    {
        using HttpClient userClient = _factory.CreateClient();
        await TestIdentityHelper.RegisterLoginAndAssignRoleAsync(_factory, userClient, "user");

        HttpResponseMessage response = await testCase.SendAsync(userClient);

        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden, testCase.Name);
    }

    [Theory]
    [MemberData(nameof(Endpoints))]
    public async Task Endpoint_WithAdminRole_ShouldNotReturnAuthErrors(EndpointAccessCase testCase)
    {
        using HttpClient adminClient = _factory.CreateClient();
        await TestIdentityHelper.RegisterLoginAndAssignRoleAsync(_factory, adminClient, "admin");

        HttpResponseMessage response = await testCase.SendAsync(adminClient);

        response.StatusCode.ShouldNotBe(HttpStatusCode.Unauthorized, testCase.Name);
        response.StatusCode.ShouldNotBe(HttpStatusCode.Forbidden, testCase.Name);
    }
}
