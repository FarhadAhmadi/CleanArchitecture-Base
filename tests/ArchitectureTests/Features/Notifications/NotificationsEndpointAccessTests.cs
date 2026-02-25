using System.Net;
using ArchitectureTests.Modules.Common;
using ArchitectureTests.Security;
using Shouldly;

namespace ArchitectureTests.Modules.Notifications;

[Collection(ArchitectureTests.Modules.Common.EndpointAccessCollection.Name)]
public sealed class NotificationsEndpointAccessTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public NotificationsEndpointAccessTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    public static IEnumerable<object[]> Endpoints()
    {
        yield return [new EndpointAccessCase("ArchiveNotification", HttpMethod.Delete, "notifications/archive/{guid}")];
        yield return [new EndpointAccessCase("CreateNotification", HttpMethod.Post, "notifications", new { channel = 4, priority = 2, recipient = "u@test.local", subject = "s", body = "b", language = "en-US", templateId = (Guid?)null, scheduledAtUtc = (DateTime?)null })];
        yield return [new EndpointAccessCase("DeleteNotificationSchedule", HttpMethod.Delete, "notifications/schedules/{guid}")];
        yield return [new EndpointAccessCase("DeleteNotificationTemplate", HttpMethod.Delete, "notification-templates/{guid}")];
        yield return [new EndpointAccessCase("GetNotification", HttpMethod.Get, "notifications/{guid}")];
        yield return [new EndpointAccessCase("GetNotificationPermissions", HttpMethod.Get, "notifications/{guid}/permissions")];
        yield return [new EndpointAccessCase("GetNotificationTemplate", HttpMethod.Get, "notification-templates/{guid}")];
        yield return [new EndpointAccessCase("ListNotifications", HttpMethod.Get, "notifications?page=1&pageSize=10")];
        yield return [new EndpointAccessCase("ListNotificationSchedules", HttpMethod.Get, "notifications/schedules")];
        yield return [new EndpointAccessCase("ListNotificationTemplates", HttpMethod.Get, "notification-templates?language=en-US")];
        yield return [new EndpointAccessCase("NotificationPermissions", HttpMethod.Put, "notifications/{guid}/permissions", new { subjectType = "User", subjectValue = Guid.NewGuid().ToString("N"), canRead = true, canManage = false })];
        yield return [new EndpointAccessCase("NotificationReportDetails", HttpMethod.Get, "notifications/reports/details?page=1&pageSize=10")];
        yield return [new EndpointAccessCase("NotificationReports", HttpMethod.Get, "notifications/reports/summary")];
        yield return [new EndpointAccessCase("NotificationScheduling", HttpMethod.Post, "notifications/{guid}/schedule", new { runAtUtc = DateTime.UtcNow.AddMinutes(5), ruleName = "r" })];
        yield return [new EndpointAccessCase("NotificationTemplates", HttpMethod.Post, "notifications/templates", new { name = "template", channel = 4, language = "en-US", subjectTemplate = "s", bodyTemplate = "b" })];
        yield return [new EndpointAccessCase("UpdateNotificationTemplate", HttpMethod.Put, "notifications/templates/{guid}", new { subjectTemplate = "s2", bodyTemplate = "b2" })];
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
