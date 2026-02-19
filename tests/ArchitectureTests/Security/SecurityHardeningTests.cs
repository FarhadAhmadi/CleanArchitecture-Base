using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Shouldly;

namespace ArchitectureTests.Security;

public sealed class SecurityHardeningTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public SecurityHardeningTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ApiResponses_ShouldIncludeSecurityHeaders_AndNoStoreCachePolicy()
    {
        using HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/api/v1/todos");

        response.Headers.Contains("X-Content-Type-Options").ShouldBeTrue();
        response.Headers.GetValues("X-Content-Type-Options").ShouldContain("nosniff");

        response.Headers.Contains("X-Frame-Options").ShouldBeTrue();
        response.Headers.GetValues("X-Frame-Options").ShouldContain("DENY");

        response.Headers.Contains("Referrer-Policy").ShouldBeTrue();
        response.Headers.GetValues("Referrer-Policy").ShouldContain("no-referrer");

        response.Headers.Contains("Content-Security-Policy").ShouldBeTrue();
        response.Headers.GetValues("Content-Security-Policy")
            .Single()
            .ShouldContain("default-src 'none'");

        response.Headers.Contains("Cache-Control").ShouldBeTrue();
        response.Headers.GetValues("Cache-Control")
            .Single()
            .ShouldContain("no-store");

        response.Headers.Contains("Server").ShouldBeFalse();
    }

    [Fact]
    public async Task UnauthorizedResponse_ShouldNotLeakTokenValidationDetails()
    {
        using HttpClient client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "not-a-valid-token");

        HttpResponseMessage response = await client.GetAsync("/api/v1/todos");
        string body = await response.Content.ReadAsStringAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
        body.ShouldNotContain("IDX");
        body.ShouldNotContain("Microsoft.IdentityModel", Case.Insensitive);
        body.ShouldNotContain("signature", Case.Insensitive);
    }

    [Fact]
    public async Task InvalidContentType_ForJsonEndpoint_ShouldReturnUnsupportedMediaType()
    {
        using HttpClient client = _factory.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/users/login")
        {
            Content = new StringContent("{\"email\":\"a@test.local\",\"password\":\"x\"}", Encoding.UTF8, "text/plain")
        };

        HttpResponseMessage response = await client.SendAsync(request);
        string body = await response.Content.ReadAsStringAsync();

        response.StatusCode.ShouldBe(HttpStatusCode.UnsupportedMediaType);
        body.ShouldContain("Unsupported media type");
    }

    [Fact]
    public async Task CorrelationId_ShouldBeSanitized_AndBounded()
    {
        using HttpClient client = _factory.CreateClient();
        string rawCorrelationId = new string('a', 80) + "!@#$";

        using var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("X-Correlation-Id", rawCorrelationId);

        HttpResponseMessage response = await client.SendAsync(request);
        string correlationId = response.Headers.GetValues("X-Correlation-Id").Single();

        correlationId.Length.ShouldBe(64);
        correlationId.All(x => char.IsLetterOrDigit(x) || x is '-' or '_' or '.').ShouldBeTrue();
    }

    [Fact]
    public async Task HealthEndpoint_ShouldExposeOnlyMinimalStatusPayload()
    {
        using HttpClient client = _factory.CreateClient();

        HttpResponseMessage response = await client.GetAsync("/health");
        string json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);

        (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.ServiceUnavailable).ShouldBeTrue();
        document.RootElement.TryGetProperty("status", out JsonElement statusElement).ShouldBeTrue();
        statusElement.GetString().ShouldNotBeNullOrWhiteSpace();
        document.RootElement.EnumerateObject().Count().ShouldBe(1);
    }

    [Fact]
    public async Task DisallowedCorsOrigin_ShouldNotReceiveAllowOriginHeader()
    {
        using HttpClient client = _factory.CreateClient();

        using var request = new HttpRequestMessage(HttpMethod.Get, "/health");
        request.Headers.Add("Origin", "https://evil.example");

        HttpResponseMessage response = await client.SendAsync(request);

        response.Headers.Contains("Access-Control-Allow-Origin").ShouldBeFalse();
    }
}
