using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ArchitectureTests.Security;
using Shouldly;

namespace ArchitectureTests.Modules.Notifications;

public sealed class NotificationsIntegrationTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public NotificationsIntegrationTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateAndGetNotification_ShouldReturnCreatedItem()
    {
        using HttpClient client = _factory.CreateClient();
        await TestIdentityHelper.RegisterLoginAndAssignRoleAsync(_factory, client, "admin");

        HttpResponseMessage create = await client.PostAsJsonAsync("/api/v1/notifications", new
        {
            channel = 4,
            priority = 2,
            recipient = "integration-user@test.local",
            subject = "subject",
            body = "body",
            language = "en-US",
            templateId = (Guid?)null,
            scheduledAtUtc = (DateTime?)null
        });

        create.StatusCode.ShouldBe(HttpStatusCode.OK);
        string createJson = await create.Content.ReadAsStringAsync();
        using var createDocument = JsonDocument.Parse(createJson);
        Guid notificationId = createDocument.RootElement.GetProperty("notificationId").GetGuid();

        HttpResponseMessage get = await client.GetAsync($"/api/v1/notifications/{notificationId}");
        get.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ScheduleNotification_ShouldBeVisibleInSchedulesEndpoint()
    {
        using HttpClient client = _factory.CreateClient();
        await TestIdentityHelper.RegisterLoginAndAssignRoleAsync(_factory, client, "admin");

        HttpResponseMessage create = await client.PostAsJsonAsync("/api/v1/notifications", new
        {
            channel = 4,
            priority = 2,
            recipient = "schedule-user@test.local",
            subject = "subject",
            body = "body",
            language = "en-US",
            templateId = (Guid?)null,
            scheduledAtUtc = (DateTime?)null
        });

        string createJson = await create.Content.ReadAsStringAsync();
        using var createDocument = JsonDocument.Parse(createJson);
        Guid notificationId = createDocument.RootElement.GetProperty("notificationId").GetGuid();

        DateTime runAtUtc = DateTime.UtcNow.AddMinutes(10);
        HttpResponseMessage schedule = await client.PostAsJsonAsync($"/api/v1/notifications/{notificationId}/schedule", new
        {
            runAtUtc,
            ruleName = "integration-rule"
        });
        schedule.StatusCode.ShouldBe(HttpStatusCode.OK);

        HttpResponseMessage listSchedules = await client.GetAsync("/api/v1/notifications/schedules");
        listSchedules.StatusCode.ShouldBe(HttpStatusCode.OK);

        string schedulesJson = await listSchedules.Content.ReadAsStringAsync();
        schedulesJson.ShouldContain(notificationId.ToString(), Case.Insensitive);
    }

    [Fact]
    public async Task NotificationReports_ShouldReturnSummaryAndDetails()
    {
        using HttpClient client = _factory.CreateClient();
        await TestIdentityHelper.RegisterLoginAndAssignRoleAsync(_factory, client, "admin");

        HttpResponseMessage summary = await client.GetAsync("/api/v1/notifications/reports/summary");
        summary.StatusCode.ShouldBe(HttpStatusCode.OK);

        HttpResponseMessage details = await client.GetAsync("/api/v1/notifications/reports/details?page=1&pageSize=10");
        details.StatusCode.ShouldBe(HttpStatusCode.OK);
    }
}
