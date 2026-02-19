using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Domain.Authorization;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ArchitectureTests.Security;

public sealed class AuthFlowTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public AuthFlowTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_Refresh_Revoke_ShouldWorkWithRotation()
    {
        HttpClient client = _factory.CreateClient();

        string email = $"user-{Guid.NewGuid():N}@test.local";
        const string password = "P@ssw0rd-123";

        HttpResponseMessage register = await client.PostAsJsonAsync("/api/v1/users/register", new
        {
            email,
            firstName = "Test",
            lastName = "User",
            password
        });

        register.StatusCode.ShouldBe(HttpStatusCode.OK);
        Guid userId = await register.Content.ReadFromJsonAsync<Guid>();
        await AttachDefaultUserRoleAsync(userId);

        HttpResponseMessage login = await client.PostAsJsonAsync("/api/v1/users/login", new { email, password });
        login.StatusCode.ShouldBe(HttpStatusCode.OK);

        (string accessToken, string refreshToken) loginTokens = await ReadTokensAsync(login);
        loginTokens.accessToken.ShouldNotBeNullOrWhiteSpace();
        loginTokens.refreshToken.ShouldNotBeNullOrWhiteSpace();

        HttpResponseMessage refresh = await client.PostAsJsonAsync(
            "/api/v1/users/refresh",
            new { loginTokens.refreshToken });

        refresh.StatusCode.ShouldBe(HttpStatusCode.OK);
        (string accessToken, string refreshToken) rotated = await ReadTokensAsync(refresh);
        rotated.refreshToken.ShouldNotBe(loginTokens.refreshToken);

        HttpResponseMessage revoke = await client.PostAsJsonAsync(
            "/api/v1/users/revoke",
            new { rotated.refreshToken });
        revoke.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        HttpResponseMessage refreshAfterRevoke = await client.PostAsJsonAsync(
            "/api/v1/users/refresh",
            new { rotated.refreshToken });
        refreshAfterRevoke.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ProtectedEndpoints_ShouldRespectPermissions()
    {
        HttpClient client = _factory.CreateClient();

        string email = $"perm-{Guid.NewGuid():N}@test.local";
        const string password = "P@ssw0rd-123";

        HttpResponseMessage register = await client.PostAsJsonAsync("/api/v1/users/register", new
        {
            email,
            firstName = "Perm",
            lastName = "User",
            password
        });

        Guid userId = await register.Content.ReadFromJsonAsync<Guid>();
        await AttachDefaultUserRoleAsync(userId);

        HttpResponseMessage login = await client.PostAsJsonAsync("/api/v1/users/login", new { email, password });
        (string accessToken, string _) tokens = await ReadTokensAsync(login);
        await ApiWebApplicationFactory.SetBearerTokenAsync(client, tokens.accessToken);

        HttpResponseMessage todos = await client.GetAsync("/api/v1/todos?page=1&pageSize=10");
        todos.StatusCode.ShouldBe(HttpStatusCode.OK);

        HttpResponseMessage usersGetById = await client.GetAsync($"/api/v1/users/{userId}");
        usersGetById.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RegisterAndLogin_NewUser_ShouldReceiveDefaultUserRolePermissions()
    {
        HttpClient client = _factory.CreateClient();

        string email = $"default-role-{Guid.NewGuid():N}@test.local";
        const string password = "P@ssw0rd-123";

        HttpResponseMessage register = await client.PostAsJsonAsync("/api/v1/users/register", new
        {
            email,
            firstName = "Default",
            lastName = "Role",
            password
        });

        register.StatusCode.ShouldBe(HttpStatusCode.OK);

        HttpResponseMessage login = await client.PostAsJsonAsync("/api/v1/users/login", new { email, password });
        login.StatusCode.ShouldBe(HttpStatusCode.OK);

        (string accessToken, string _) tokens = await ReadTokensAsync(login);
        await ApiWebApplicationFactory.SetBearerTokenAsync(client, tokens.accessToken);

        HttpResponseMessage todos = await client.GetAsync("/api/v1/todos?page=1&pageSize=10");
        todos.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    private async Task AttachDefaultUserRoleAsync(Guid userId)
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        Guid userRoleId = await dbContext.Roles
            .Where(r => r.Name == "user")
            .Select(r => r.Id)
            .SingleAsync();

        bool exists = await dbContext.UserRoles.AnyAsync(x => x.UserId == userId && x.RoleId == userRoleId);
        if (!exists)
        {
            dbContext.UserRoles.Add(new UserRole { UserId = userId, RoleId = userRoleId });
            await dbContext.SaveChangesAsync();
        }
    }

    private static async Task<(string accessToken, string refreshToken)> ReadTokensAsync(HttpResponseMessage response)
    {
        string json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);
        string accessToken = document.RootElement.GetProperty("accessToken").GetString()!;
        string refreshToken = document.RootElement.GetProperty("refreshToken").GetString()!;
        return (accessToken, refreshToken);
    }
}
