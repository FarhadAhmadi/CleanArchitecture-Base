using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Domain.Users;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ArchitectureTests.Security;

public sealed class IdempotencyMiddlewareTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public IdempotencyMiddlewareTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task DuplicateWriteRequest_WithSameIdempotencyKey_ShouldReplayFirstResponse()
    {
        using HttpClient client = _factory.CreateClient();

        string email = $"idempotency-{Guid.NewGuid():N}@test.local";
        var payload = new
        {
            email,
            firstName = "Idempotent",
            lastName = "User",
            password = "P@ssw0rd-123"
        };

        HttpResponseMessage first = await SendRegisterAsync(client, payload, "register-once-key");
        HttpResponseMessage second = await SendRegisterAsync(client, payload, "register-once-key");

        first.StatusCode.ShouldBe(HttpStatusCode.OK);
        second.StatusCode.ShouldBe(HttpStatusCode.OK);

        string firstBody = await first.Content.ReadAsStringAsync();
        string secondBody = await second.Content.ReadAsStringAsync();
        Guid firstUserId = JsonDocument.Parse(firstBody).RootElement.GetGuid();
        Guid secondUserId = JsonDocument.Parse(secondBody).RootElement.GetGuid();
        secondUserId.ShouldBe(firstUserId);

        using IServiceScope scope = _factory.Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        int count = await dbContext.Users.CountAsync(x => x.Email == email);
        count.ShouldBe(1);
    }

    private static async Task<HttpResponseMessage> SendRegisterAsync(HttpClient client, object payload, string idempotencyKey)
    {
        using HttpRequestMessage request = new(HttpMethod.Post, "/api/v1/users/register")
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Add("Idempotency-Key", idempotencyKey);
        return await client.SendAsync(request);
    }
}
