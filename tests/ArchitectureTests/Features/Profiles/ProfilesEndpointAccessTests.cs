using System.Net;
using ArchitectureTests.Modules.Common;
using ArchitectureTests.Security;
using Shouldly;

namespace ArchitectureTests.Modules.Profiles;

[Collection(ArchitectureTests.Modules.Common.EndpointAccessCollection.Name)]
public sealed class ProfilesEndpointAccessTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;
    private static readonly string[] DefaultInterests = ["coding"];

    public ProfilesEndpointAccessTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    public static IEnumerable<object[]> Endpoints()
    {
        yield return [new EndpointAccessCase("CreateMyProfile", HttpMethod.Post, "profiles/me", new { displayName = "name", preferredLanguage = "en-US", isProfilePublic = true })];
        yield return [new EndpointAccessCase("AddMyProfileInterests", HttpMethod.Post, "profiles/me/interests", new { interests = DefaultInterests })];
        yield return [new EndpointAccessCase("DeleteMyProfileAvatar", HttpMethod.Delete, "profiles/me/avatar")];
        yield return [new EndpointAccessCase("GetMyProfile", HttpMethod.Get, "profiles/me")];
        yield return [new EndpointAccessCase("GetProfilesAdminReport", HttpMethod.Get, "profiles/reports/admin?page=1&pageSize=10")];
        yield return [new EndpointAccessCase("GetPublicProfile", HttpMethod.Get, "profiles/{guid}/public")];
        yield return [new EndpointAccessCase("ManageMyProfileMusic", HttpMethod.Get, "profiles/me/music")];
        yield return [new EndpointAccessCase("RemoveMyProfileInterest", HttpMethod.Delete, "profiles/me/interests/test-interest")];
        yield return [new EndpointAccessCase("UpdateMyProfileAvatar", HttpMethod.Put, "profiles/me/avatar", new { avatarFileId = Guid.NewGuid() })];
        yield return [new EndpointAccessCase("UpdateMyProfileBasic", HttpMethod.Put, "profiles/me/basic", new { displayName = "name", bio = "bio", dateOfBirth = (DateTime?)null, gender = "n/a", location = "test" })];
        yield return [new EndpointAccessCase("UpdateMyProfileBio", HttpMethod.Patch, "profiles/me/bio", new { bio = "bio" })];
        yield return [new EndpointAccessCase("UpdateMyProfileContact", HttpMethod.Patch, "profiles/me/contact", new { contactEmail = "c@test.local", contactPhone = "0912", website = "https://example.com", timeZone = "UTC" })];
        yield return [new EndpointAccessCase("UpdateMyProfileMusic", HttpMethod.Put, "profiles/me/music", new { musicTitle = "t", musicArtist = "a", musicFileId = Guid.NewGuid() })];
        yield return [new EndpointAccessCase("UpdateMyProfilePreferences", HttpMethod.Patch, "profiles/me/preferences", new { preferredLanguage = "en-US", receiveSecurityAlerts = true, receiveProductUpdates = false })];
        yield return [new EndpointAccessCase("UpdateMyProfilePrivacy", HttpMethod.Patch, "profiles/me/privacy", new { isProfilePublic = true, showEmail = false, showPhone = false })];
        yield return [new EndpointAccessCase("UpdateMyProfileSocialLinks", HttpMethod.Patch, "profiles/me/social-links", new { links = new Dictionary<string, string> { ["github"] = "https://github.com/test" } })];
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
