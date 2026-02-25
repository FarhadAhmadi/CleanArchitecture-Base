using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Application.Abstractions.Scheduler;
using ArchitectureTests.Modules.Common;
using ArchitectureTests.Security;
using Domain.Modules.Scheduler;
using Domain.Scheduler;
using Infrastructure.Scheduler;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace ArchitectureTests.Modules.Scheduler;

[Collection(EndpointAccessCollection.Name)]
public sealed class SchedulerRuntimeBehaviorTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public SchedulerRuntimeBehaviorTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ExecutionService_WhenJobExceedsTimeout_ShouldReturnTimedOut()
    {
        var job = new ScheduledJob
        {
            Id = Guid.NewGuid(),
            Name = "timeout-direct",
            Type = JobType.GenericNoOp,
            PayloadJson = "{\"note\":\"slow\",\"delaySeconds\":2}",
            MaxExecutionSeconds = 1
        };

        IScheduledJobHandler[] handlers = [new GenericNoOpScheduledJobHandler(NullLogger<GenericNoOpScheduledJobHandler>.Instance)];
        var service = new SchedulerExecutionService(
            handlers,
            new SchedulerPayloadValidator(),
            NullLogger<SchedulerExecutionService>.Instance);

        SchedulerExecutionResult result = await service.ExecuteAsync(
            new SchedulerExecutionRequest(
                job,
                "test",
                "test-node",
                1,
                1,
                DateTime.UtcNow,
                false),
            CancellationToken.None);

        result.Status.ShouldBe(JobExecutionStatus.TimedOut);
    }

    [Fact]
    public async Task UpsertSchedule_WithInvalidCron_ShouldReturnBadRequest()
    {
        using HttpClient client = _factory.CreateClient();
        await TestIdentityHelper.RegisterLoginAndAssignRoleAsync(_factory, client, "admin");

        HttpResponseMessage create = await client.PostAsJsonAsync("/api/v1/scheduler/jobs", new
        {
            name = $"invalid-cron-job-{Guid.NewGuid():N}",
            description = "cron validation",
            type = 1,
            payloadJson = "{\"note\":\"x\"}",
            maxRetryAttempts = 2,
            retryBackoffSeconds = 1,
            maxExecutionSeconds = 30,
            maxConsecutiveFailures = 3
        });
        create.StatusCode.ShouldBe(HttpStatusCode.OK);
        Guid jobId = await ReadJobIdAsync(create);

        HttpResponseMessage schedule = await client.PostAsJsonAsync($"/api/v1/scheduler/jobs/{jobId}/schedule", new
        {
            type = 1,
            cronExpression = "invalid cron expression",
            intervalSeconds = (int?)null,
            oneTimeAtUtc = (DateTime?)null,
            startAtUtc = (DateTime?)null,
            endAtUtc = (DateTime?)null,
            misfirePolicy = 1,
            maxCatchUpRuns = 3
        });

        schedule.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DistributedLock_WhenRacingForSameKey_ShouldGrantSingleWinner()
    {
        using IServiceScope scope1 = _factory.Services.CreateScope();
        using IServiceScope scope2 = _factory.Services.CreateScope();

        ISchedulerDistributedLockProvider provider1 = scope1.ServiceProvider.GetRequiredService<ISchedulerDistributedLockProvider>();
        ISchedulerDistributedLockProvider provider2 = scope2.ServiceProvider.GetRequiredService<ISchedulerDistributedLockProvider>();

        string lockName = $"scheduler:test-lock:{Guid.NewGuid():N}";
        Task<bool> t1 = provider1.TryAcquireAsync(lockName, TimeSpan.FromSeconds(15), CancellationToken.None);
        Task<bool> t2 = provider2.TryAcquireAsync(lockName, TimeSpan.FromSeconds(15), CancellationToken.None);

        bool[] outcomes = await Task.WhenAll(t1, t2);
        outcomes.Count(x => x).ShouldBe(1);

        await provider1.ReleaseAsync(lockName, CancellationToken.None);
        await provider2.ReleaseAsync(lockName, CancellationToken.None);
    }

    [Fact]
    public async Task SchedulerEndpoints_BulkCreateAndList_ShouldRemainResponsive()
    {
        using HttpClient client = _factory.CreateClient();
        await TestIdentityHelper.RegisterLoginAndAssignRoleAsync(_factory, client, "admin");

        for (int i = 0; i < 20; i++)
        {
            HttpResponseMessage create = await client.PostAsJsonAsync("/api/v1/scheduler/jobs", new
            {
                name = $"bulk-job-{Guid.NewGuid():N}",
                description = "load",
                type = 1,
                payloadJson = "{\"note\":\"bulk\"}",
                maxRetryAttempts = 2,
                retryBackoffSeconds = 1,
                maxExecutionSeconds = 10,
                maxConsecutiveFailures = 3
            });
            create.StatusCode.ShouldBe(HttpStatusCode.OK);
        }

        HttpResponseMessage list = await client.GetAsync("/api/v1/scheduler/jobs?page=1&pageSize=50");
        list.StatusCode.ShouldBe(HttpStatusCode.OK);
        string body = await list.Content.ReadAsStringAsync();
        body.ShouldContain("\"items\"");
    }

    private static async Task<Guid> ReadJobIdAsync(HttpResponseMessage response)
    {
        string json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("id").GetGuid();
    }
}
