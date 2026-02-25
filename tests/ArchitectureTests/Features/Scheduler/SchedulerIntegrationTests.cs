using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ArchitectureTests.Security;
using Shouldly;

namespace ArchitectureTests.Modules.Scheduler;

public sealed class SchedulerIntegrationTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public SchedulerIntegrationTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateScheduleRunAndReadLogs_ShouldSucceed()
    {
        using HttpClient client = _factory.CreateClient();
        await TestIdentityHelper.RegisterLoginAndAssignRoleAsync(_factory, client, "admin");

        HttpResponseMessage create = await client.PostAsJsonAsync("/api/v1/scheduler/jobs", new
        {
            name = $"integration-job-{Guid.NewGuid():N}",
            description = "integration",
            type = 3,
            payloadJson = "{\"recipient\":\"integration-user@test.local\",\"subject\":\"int-probe\"}",
            maxRetryAttempts = 3,
            retryBackoffSeconds = 3,
            maxExecutionSeconds = 20,
            maxConsecutiveFailures = 5
        });
        create.StatusCode.ShouldBe(HttpStatusCode.OK);

        string createJson = await create.Content.ReadAsStringAsync();
        using var createDoc = JsonDocument.Parse(createJson);
        Guid jobId = createDoc.RootElement.GetProperty("id").GetGuid();

        HttpResponseMessage schedule = await client.PostAsJsonAsync($"/api/v1/scheduler/jobs/{jobId}/schedule", new
        {
            type = 2,
            cronExpression = (string?)null,
            intervalSeconds = 120,
            oneTimeAtUtc = (DateTime?)null,
            startAtUtc = (DateTime?)null,
            endAtUtc = (DateTime?)null,
            misfirePolicy = 1,
            maxCatchUpRuns = 10
        });
        schedule.StatusCode.ShouldBe(HttpStatusCode.OK);

        HttpResponseMessage run = await client.PostAsJsonAsync($"/api/v1/scheduler/jobs/{jobId}/run", new { });
        run.StatusCode.ShouldBe(HttpStatusCode.OK);

        HttpResponseMessage logs = await client.GetAsync($"/api/v1/scheduler/jobs/{jobId}/logs?page=1&pageSize=10");
        logs.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
