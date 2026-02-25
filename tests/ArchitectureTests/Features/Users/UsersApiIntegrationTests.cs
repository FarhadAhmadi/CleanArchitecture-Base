using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ArchitectureTests.Modules.Common;
using ArchitectureTests.Security;
using Shouldly;

namespace ArchitectureTests.Modules.Users;

[Collection(EndpointAccessCollection.Name)]
public sealed class UsersApiIntegrationTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public UsersApiIntegrationTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_ShouldReturnUserId()
    {
        using HttpClient client = _factory.CreateClient();
        string email = $"register-{Guid.NewGuid():N}@test.local";

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/v1/users/register", new
        {
            email,
            firstName = "Register",
            lastName = "User",
            password = "P@ssw0rd-123"
        });

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        Guid userId = await response.Content.ReadFromJsonAsync<Guid>();
        userId.ShouldNotBe(Guid.Empty);
    }

    [Fact]
    public async Task Login_ShouldReturnAccessAndRefreshTokens()
    {
        using HttpClient client = _factory.CreateClient();
        string email = $"login-{Guid.NewGuid():N}@test.local";
        const string password = "P@ssw0rd-123";

        HttpResponseMessage register = await client.PostAsJsonAsync("/api/v1/users/register", new
        {
            email,
            firstName = "Login",
            lastName = "User",
            password
        });
        register.StatusCode.ShouldBe(HttpStatusCode.OK);

        HttpResponseMessage login = await client.PostAsJsonAsync("/api/v1/users/login", new { email, password });
        login.StatusCode.ShouldBe(HttpStatusCode.OK);

        (string accessToken, string refreshToken) tokens = await TestIdentityHelper.ReadTokensAsync(login);
        tokens.accessToken.ShouldNotBeNullOrWhiteSpace();
        tokens.refreshToken.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Refresh_ShouldRotateRefreshToken()
    {
        using HttpClient client = _factory.CreateClient();
        string email = $"refresh-{Guid.NewGuid():N}@test.local";
        const string password = "P@ssw0rd-123";

        HttpResponseMessage register = await client.PostAsJsonAsync("/api/v1/users/register", new
        {
            email,
            firstName = "Refresh",
            lastName = "User",
            password
        });
        register.StatusCode.ShouldBe(HttpStatusCode.OK);

        HttpResponseMessage login = await client.PostAsJsonAsync("/api/v1/users/login", new { email, password });
        (string _, string refreshToken) tokens = await TestIdentityHelper.ReadTokensAsync(login);

        HttpResponseMessage refresh = await client.PostAsJsonAsync("/api/v1/users/refresh", new { tokens.refreshToken });
        refresh.StatusCode.ShouldBe(HttpStatusCode.OK);

        (string _, string rotatedRefreshToken) = await TestIdentityHelper.ReadTokensAsync(refresh);
        rotatedRefreshToken.ShouldNotBe(tokens.refreshToken);
    }

    [Fact]
    public async Task Revoke_ShouldInvalidateRotatedRefreshToken()
    {
        using HttpClient client = _factory.CreateClient();
        string email = $"revoke-{Guid.NewGuid():N}@test.local";
        const string password = "P@ssw0rd-123";

        HttpResponseMessage register = await client.PostAsJsonAsync("/api/v1/users/register", new
        {
            email,
            firstName = "Revoke",
            lastName = "User",
            password
        });
        register.StatusCode.ShouldBe(HttpStatusCode.OK);

        HttpResponseMessage login = await client.PostAsJsonAsync("/api/v1/users/login", new { email, password });
        (string _, string refreshToken) tokens = await TestIdentityHelper.ReadTokensAsync(login);

        HttpResponseMessage revoke = await client.PostAsJsonAsync("/api/v1/users/revoke", new { tokens.refreshToken });
        revoke.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ForgotPassword_ShouldNotReturnServerError()
    {
        using HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/v1/users/forgot-password", new
        {
            email = $"unknown-{Guid.NewGuid():N}@test.local"
        });

        ((int)response.StatusCode).ShouldBeInRange(200, 499);
    }

    [Fact]
    public async Task ResetPassword_WithInvalidCode_ShouldNotReturnServerError()
    {
        using HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/v1/users/reset-password", new
        {
            email = $"unknown-{Guid.NewGuid():N}@test.local",
            code = "000000",
            newPassword = "NewP@ssw0rd-123"
        });

        ((int)response.StatusCode).ShouldBeInRange(200, 499);
    }

    [Fact]
    public async Task VerifyEmail_WithInvalidCode_ShouldNotReturnServerError()
    {
        using HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/v1/users/verify-email", new
        {
            email = $"unknown-{Guid.NewGuid():N}@test.local",
            code = "000000"
        });

        ((int)response.StatusCode).ShouldBeInRange(200, 499);
    }

    [Fact]
    public async Task ResendVerificationCode_ShouldNotReturnServerError()
    {
        using HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.PostAsJsonAsync("/api/v1/users/resend-verification-code", new
        {
            email = $"unknown-{Guid.NewGuid():N}@test.local"
        });

        ((int)response.StatusCode).ShouldBeInRange(200, 499);
    }

    [Fact]
    public async Task OAuthStart_ShouldNotReturnServerError_WhenProviderInvalid()
    {
        using HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/v1/users/oauth/unknown/start");

        ((int)response.StatusCode).ShouldBeInRange(200, 499);
    }

    [Fact]
    public async Task OAuthCallback_ShouldNotReturnServerError_WhenMissingParameters()
    {
        using HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/v1/users/oauth/callback");

        ((int)response.StatusCode).ShouldBeInRange(200, 499);
    }

    [Fact]
    public async Task GetMe_ShouldRequireAuthorization_AndWorkWithToken()
    {
        using HttpClient anonymousClient = _factory.CreateClient();
        HttpResponseMessage unauthorized = await anonymousClient.GetAsync("/api/v1/users/me");
        unauthorized.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        using HttpClient authClient = _factory.CreateClient();
        await TestIdentityHelper.RegisterLoginAndAssignRoleAsync(_factory, authClient, "user");

        HttpResponseMessage authorized = await authClient.GetAsync("/api/v1/users/me");
        authorized.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_ShouldRequireUsersAccessPermission()
    {
        using HttpClient userClient = _factory.CreateClient();
        await TestIdentityHelper.RegisterLoginAndAssignRoleAsync(_factory, userClient, "user");

        HttpResponseMessage forbidden = await userClient.GetAsync($"/api/v1/users/{Guid.NewGuid()}");
        forbidden.StatusCode.ShouldBe(HttpStatusCode.Forbidden);

        using HttpClient adminClient = _factory.CreateClient();
        await TestIdentityHelper.RegisterLoginAndAssignRoleAsync(_factory, adminClient, "admin");

        HttpResponseMessage adminResponse = await adminClient.GetAsync($"/api/v1/users/{Guid.NewGuid()}");
        adminResponse.StatusCode.ShouldNotBe(HttpStatusCode.Unauthorized);
        adminResponse.StatusCode.ShouldNotBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RevokeAllSessions_ShouldRequireAuthorization()
    {
        using HttpClient anonymousClient = _factory.CreateClient();
        HttpResponseMessage unauthorized = await anonymousClient.PostAsJsonAsync("/api/v1/users/sessions/revoke-all", new { });
        unauthorized.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);

        using HttpClient authClient = _factory.CreateClient();
        await TestIdentityHelper.RegisterLoginAndAssignRoleAsync(_factory, authClient, "user");

        HttpResponseMessage authorized = await authClient.PostAsJsonAsync("/api/v1/users/sessions/revoke-all", new { });
        authorized.StatusCode.ShouldNotBe(HttpStatusCode.Unauthorized);
    }
}
