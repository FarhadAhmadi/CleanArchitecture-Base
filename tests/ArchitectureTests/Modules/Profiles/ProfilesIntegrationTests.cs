using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ArchitectureTests.Security;
using Shouldly;

namespace ArchitectureTests.Modules.Profiles;

public sealed class ProfilesIntegrationTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public ProfilesIntegrationTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetMyProfile_WithoutToken_ShouldReturnUnauthorized()
    {
        using HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/v1/profiles/me");

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateAndUpdateProfile_ShouldReturnUpdatedData()
    {
        using HttpClient client = _factory.CreateClient();
        await TestIdentityHelper.RegisterLoginAndAssignRoleAsync(_factory, client, "admin");

        HttpResponseMessage create = await client.PostAsJsonAsync("/api/v1/profiles/me", new
        {
            displayName = "Integration User",
            preferredLanguage = "en-US",
            isProfilePublic = true
        });
        create.StatusCode.ShouldBe(HttpStatusCode.OK);

        HttpResponseMessage update = await client.PutAsJsonAsync("/api/v1/profiles/me/basic", new
        {
            displayName = "Updated Integration User",
            bio = "Hello from tests",
            dateOfBirth = (DateTime?)null,
            gender = "n/a",
            location = "TestCity"
        });
        update.StatusCode.ShouldBe(HttpStatusCode.OK);

        HttpResponseMessage get = await client.GetAsync("/api/v1/profiles/me");
        get.StatusCode.ShouldBe(HttpStatusCode.OK);

        string json = await get.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);
        document.RootElement.GetProperty("displayName").GetString().ShouldBe("Updated Integration User");
        document.RootElement.GetProperty("bio").GetString().ShouldBe("Hello from tests");
        document.RootElement.GetProperty("location").GetString().ShouldBe("TestCity");
    }

    [Fact]
    public async Task ProfilesAdminReport_ShouldRequireAdminPermission()
    {
        using HttpClient userClient = _factory.CreateClient();
        await TestIdentityHelper.RegisterLoginAndAssignRoleAsync(_factory, userClient, "user");

        HttpResponseMessage forbidden = await userClient.GetAsync("/api/v1/profiles/reports/admin?page=1&pageSize=10");
        forbidden.StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        using HttpClient adminClient = _factory.CreateClient();
        await TestIdentityHelper.RegisterLoginAndAssignRoleAsync(_factory, adminClient, "admin");

        HttpResponseMessage ok = await adminClient.GetAsync("/api/v1/profiles/reports/admin?page=1&pageSize=10");
        ok.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
