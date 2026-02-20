using System.Net.Http.Headers;
using Domain.Authorization;
using Domain.Users;
using Infrastructure.Files;
using Infrastructure.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ArchitectureTests.Security;

public sealed class ApiWebApplicationFactory : WebApplicationFactory<Web.Api.Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();
            services.RemoveAll<DbContextOptions<ApplicationReadDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ApplicationReadDbContext>>();
            services.RemoveAll<ApplicationDbContext>();
            services.RemoveAll<ApplicationReadDbContext>();
            services.RemoveAll<Application.Abstractions.Data.IApplicationDbContext>();
            services.RemoveAll<Application.Abstractions.Data.IApplicationReadDbContext>();
            services.RemoveAll<IFileObjectStorage>();

            _connection.Open();

            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(_connection));
            services.AddDbContext<ApplicationReadDbContext>(options => options.UseSqlite(_connection));
            services.AddScoped<Application.Abstractions.Data.IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
            services.AddScoped<Application.Abstractions.Data.IApplicationReadDbContext>(sp => sp.GetRequiredService<ApplicationReadDbContext>());
            services.AddSingleton<IFileObjectStorage, FakeFileObjectStorage>();

            using IServiceScope scope = services.BuildServiceProvider().CreateScope();
            ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.EnsureCreated();

            SeedAuthorization(dbContext);
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            _connection.Dispose();
        }
    }

    internal static async Task SetBearerTokenAsync(HttpClient client, string accessToken)
    {
        await Task.Yield();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }

    private static void SeedAuthorization(ApplicationDbContext dbContext)
    {
        if (dbContext.Roles.Any() || dbContext.Permissions.Any())
        {
            return;
        }

        var userRole = new Role { Id = Guid.NewGuid(), Name = "user" };
        var adminRole = new Role { Id = Guid.NewGuid(), Name = "admin" };

        var todosRead = new Permission { Id = Guid.NewGuid(), Code = PermissionCodes.TodosRead, Description = "todos read" };
        var todosWrite = new Permission { Id = Guid.NewGuid(), Code = PermissionCodes.TodosWrite, Description = "todos write" };
        var usersAccess = new Permission { Id = Guid.NewGuid(), Code = PermissionCodes.UsersAccess, Description = "users access" };
        var authManage = new Permission { Id = Guid.NewGuid(), Code = PermissionCodes.AuthorizationManage, Description = "authorization manage" };

        dbContext.Roles.AddRange(userRole, adminRole);
        dbContext.Permissions.AddRange(todosRead, todosWrite, usersAccess, authManage);

        dbContext.RolePermissions.AddRange(
            new RolePermission { RoleId = userRole.Id, PermissionId = todosRead.Id },
            new RolePermission { RoleId = userRole.Id, PermissionId = todosWrite.Id },
            new RolePermission { RoleId = adminRole.Id, PermissionId = todosRead.Id },
            new RolePermission { RoleId = adminRole.Id, PermissionId = todosWrite.Id },
            new RolePermission { RoleId = adminRole.Id, PermissionId = usersAccess.Id },
            new RolePermission { RoleId = adminRole.Id, PermissionId = authManage.Id });

        dbContext.SaveChanges();
    }

    private sealed class FakeFileObjectStorage : IFileObjectStorage
    {
        public Task UploadAsync(string objectKey, Stream content, long contentLength, string contentType, CancellationToken cancellationToken)
        {
            _ = objectKey;
            _ = content;
            _ = contentLength;
            _ = contentType;
            _ = cancellationToken;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(string objectKey, CancellationToken cancellationToken)
        {
            _ = objectKey;
            _ = cancellationToken;
            return Task.CompletedTask;
        }

        public Task<string> CreatePresignedDownloadUrlAsync(string objectKey, int expirySeconds, CancellationToken cancellationToken)
        {
            _ = objectKey;
            _ = expirySeconds;
            _ = cancellationToken;
            return Task.FromResult("https://example.local/files/presigned");
        }

        public Task<(Stream Content, string ContentType)> OpenReadAsync(string objectKey, CancellationToken cancellationToken)
        {
            _ = objectKey;
            _ = cancellationToken;
            Stream stream = new MemoryStream();
            return Task.FromResult((stream, "application/octet-stream"));
        }
    }
}
