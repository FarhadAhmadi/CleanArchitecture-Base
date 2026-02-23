using System.Net;
using System.Text.Json;
using ArchitectureTests.Security;
using Domain.Authorization;
using Domain.Files;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ArchitectureTests.Modules.Files;

public sealed class FilesRolePermissionVisibilityTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory _factory;

    public FilesRolePermissionVisibilityTests(ApiWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SearchFiles_ShouldIncludeRoleBasedReadableFile()
    {
        const string roleName = "files-reader";
        await TestAuthorizationHelper.EnsureRoleWithPermissionsAsync(_factory, roleName, PermissionCodes.FilesRead);

        using HttpClient memberClient = _factory.CreateClient();
        (Guid memberId, _, _) = await TestIdentityHelper.RegisterLoginAndAssignRoleAsync(_factory, memberClient, roleName);

        var fileId = Guid.NewGuid();
        await SeedRoleReadableFileAsync(fileId, roleName, memberId);

        HttpResponseMessage response = await memberClient.GetAsync("/api/v1/files/search?page=1&pageSize=20");
        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        string json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);
        JsonElement items = document.RootElement.GetProperty("items");
        items.GetArrayLength().ShouldBeGreaterThan(0);
        json.ShouldContain(fileId.ToString(), Case.Insensitive);
    }

    [Fact]
    public async Task DeleteFile_ShouldSoftDeleteFirst_AndLeaveStorageCleanupForWorker()
    {
        const string roleName = "files-deleter";
        await TestAuthorizationHelper.EnsureRoleWithPermissionsAsync(
            _factory,
            roleName,
            PermissionCodes.FilesDelete);

        using HttpClient memberClient = _factory.CreateClient();
        (Guid memberId, _, _) = await TestIdentityHelper.RegisterLoginAndAssignRoleAsync(_factory, memberClient, roleName);

        var fileId = Guid.NewGuid();
        await SeedOwnedFileAsync(fileId, memberId);

        HttpResponseMessage response = await memberClient.DeleteAsync($"/api/v1/files/{fileId}");
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        using IServiceScope scope = _factory.Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        FileAsset? deleted = await dbContext.FileAssets.SingleOrDefaultAsync(x => x.Id == fileId);
        deleted.ShouldNotBeNull();
        deleted.IsDeleted.ShouldBeTrue();
        deleted.DeletedAtUtc.ShouldNotBeNull();
        deleted.StorageDeletedAtUtc.ShouldBeNull();
    }

    private async Task SeedRoleReadableFileAsync(Guid fileId, string roleName, Guid ownerUserId)
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        dbContext.FileAssets.Add(CreateFileAsset(fileId, ownerUserId));

        dbContext.FilePermissionEntries.Add(new FilePermissionEntry
        {
            Id = Guid.NewGuid(),
            FileId = fileId,
            SubjectType = "Role",
            SubjectValue = roleName,
            CanRead = true,
            CanWrite = false,
            CanDelete = false,
            CreatedAtUtc = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync();
    }

    private async Task SeedOwnedFileAsync(Guid fileId, Guid ownerUserId)
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.FileAssets.Add(CreateFileAsset(fileId, ownerUserId));
        await dbContext.SaveChangesAsync();
    }

    private static FileAsset CreateFileAsset(Guid fileId, Guid ownerUserId)
    {
        return new FileAsset
        {
            Id = fileId,
            OwnerUserId = ownerUserId,
            ObjectKey = $"files/tests/{Guid.NewGuid():N}.txt",
            FileName = "role-visible.txt",
            Extension = ".TXT",
            ContentType = "text/plain",
            SizeBytes = 12,
            Module = "General",
            Sha256 = new string('A', 64),
            IsScanned = true,
            UploadedAtUtc = DateTime.UtcNow
        };
    }
}
