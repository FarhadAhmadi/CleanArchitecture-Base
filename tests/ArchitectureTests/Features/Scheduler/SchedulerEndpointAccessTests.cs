using System.Net;
using ArchitectureTests.Modules.Common;
using ArchitectureTests.Security;
using Shouldly;

namespace ArchitectureTests.Modules.Scheduler;

[Collection(EndpointAccessCollection.Name)]
public sealed class SchedulerEndpointAccessTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public SchedulerEndpointAccessTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    public static IEnumerable<object[]> Endpoints()
    {
        yield return [new EndpointAccessCase("CreateJob", HttpMethod.Post, "scheduler/jobs", new
        {
            name = $"job-{Guid.NewGuid():N}",
            description = "d",
            type = 1,
            payloadJson = "{}",
            maxRetryAttempts = 3,
            retryBackoffSeconds = 10,
            maxExecutionSeconds = 30,
            maxConsecutiveFailures = 5
        })];
        yield return [new EndpointAccessCase("ListJobs", HttpMethod.Get, "scheduler/jobs?page=1&pageSize=10")];
        yield return [new EndpointAccessCase("GetJobById", HttpMethod.Get, "scheduler/jobs/{guid}")];
        yield return [new EndpointAccessCase("UpdateJob", HttpMethod.Put, "scheduler/jobs/{guid}", new
        {
            name = $"job-{Guid.NewGuid():N}",
            description = "u",
            type = 2,
            payloadJson = "{\"retentionDays\":7}",
            maxRetryAttempts = 2,
            retryBackoffSeconds = 20,
            maxExecutionSeconds = 60,
            maxConsecutiveFailures = 5
        })];
        yield return [new EndpointAccessCase("DisableJob", HttpMethod.Delete, "scheduler/jobs/{guid}")];
        yield return [new EndpointAccessCase("UpsertSchedule", HttpMethod.Post, "scheduler/jobs/{guid}/schedule", new
        {
            type = 2,
            cronExpression = (string?)null,
            intervalSeconds = 60,
            oneTimeAtUtc = (DateTime?)null,
            startAtUtc = (DateTime?)null,
            endAtUtc = (DateTime?)null,
            misfirePolicy = 1,
            maxCatchUpRuns = 5
        })];
        yield return [new EndpointAccessCase("GetSchedule", HttpMethod.Get, "scheduler/jobs/{guid}/schedule")];
        yield return [new EndpointAccessCase("DeleteSchedule", HttpMethod.Delete, "scheduler/jobs/{guid}/schedule")];
        yield return [new EndpointAccessCase("RunNow", HttpMethod.Post, "scheduler/jobs/{guid}/run", new { })];
        yield return [new EndpointAccessCase("Pause", HttpMethod.Post, "scheduler/jobs/{guid}/pause", new { })];
        yield return [new EndpointAccessCase("Resume", HttpMethod.Post, "scheduler/jobs/{guid}/resume", new { })];
        yield return [new EndpointAccessCase("Replay", HttpMethod.Post, "scheduler/jobs/{guid}/replay", new { })];
        yield return [new EndpointAccessCase("Quarantine", HttpMethod.Post, "scheduler/jobs/{guid}/quarantine", new { quarantineMinutes = 30, reason = "test" })];
        yield return [new EndpointAccessCase("GetJobLogs", HttpMethod.Get, "scheduler/jobs/{guid}/logs?page=1&pageSize=10")];
        yield return [new EndpointAccessCase("GetAllLogs", HttpMethod.Get, "scheduler/jobs/logs?page=1&pageSize=10")];
        yield return [new EndpointAccessCase("GetReport", HttpMethod.Get, "scheduler/jobs/report?format=csv")];
        yield return [new EndpointAccessCase("GetPermissions", HttpMethod.Get, "scheduler/jobs/{guid}/permissions")];
        yield return [new EndpointAccessCase("UpsertPermissions", HttpMethod.Put, "scheduler/jobs/{guid}/permissions", new { subjectType = "User", subjectValue = Guid.NewGuid().ToString("N"), canRead = true, canManage = true })];
        yield return [new EndpointAccessCase("AddDependency", HttpMethod.Post, "scheduler/jobs/{guid}/dependencies", new { dependsOnJobId = Guid.NewGuid() })];
        yield return [new EndpointAccessCase("GetDependencies", HttpMethod.Get, "scheduler/jobs/{guid}/dependencies")];
        yield return [new EndpointAccessCase("RemoveDependency", HttpMethod.Delete, "scheduler/jobs/{guid}/dependencies/{guid}")];
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
