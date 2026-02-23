using System.Net;
using System.Text.Json;
using ArchitectureTests.Security;
using Domain.Authorization;
using Domain.Modules.Notifications;
using Domain.Notifications;
using Infrastructure.Database;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ArchitectureTests.Modules.Notifications;

public sealed class NotificationsRolePermissionVisibilityTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public NotificationsRolePermissionVisibilityTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ListNotifications_ShouldIncludeRoleBasedReadableNotification()
    {
        const string roleName = "notifications-reader";
        await TestAuthorizationHelper.EnsureRoleWithPermissionsAsync(_factory, roleName, PermissionCodes.NotificationsRead);

        using HttpClient memberClient = _factory.CreateClient();
        (Guid memberId, _, _) = await TestIdentityHelper.RegisterLoginAndAssignRoleAsync(_factory, memberClient, roleName);

        var notificationId = Guid.NewGuid();
        await SeedRoleReadableNotificationAsync(notificationId, roleName, memberId);

        HttpResponseMessage response = await memberClient.GetAsync("/api/v1/notifications?page=1&pageSize=20");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        string json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);
        JsonElement items = document.RootElement.GetProperty("items");
        items.GetArrayLength().ShouldBeGreaterThan(0);
        json.ShouldContain(notificationId.ToString(), Case.Insensitive);
    }

    private async Task SeedRoleReadableNotificationAsync(Guid notificationId, string roleName, Guid ownerUserId)
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        dbContext.NotificationMessages.Add(new NotificationMessage
        {
            Id = notificationId,
            CreatedByUserId = ownerUserId,
            Channel = NotificationChannel.InApp,
            Priority = NotificationPriority.Medium,
            Status = NotificationStatus.Pending,
            RecipientEncrypted = "encrypted",
            RecipientHash = "hash",
            Subject = "role-visible",
            Body = "body",
            Language = "en-US",
            CreatedAtUtc = DateTime.UtcNow
        });

        dbContext.NotificationPermissionEntries.Add(new NotificationPermissionEntry
        {
            Id = Guid.NewGuid(),
            NotificationId = notificationId,
            SubjectType = "Role",
            SubjectValue = roleName,
            CanRead = true,
            CanManage = false,
            CreatedAtUtc = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync();
    }
}
