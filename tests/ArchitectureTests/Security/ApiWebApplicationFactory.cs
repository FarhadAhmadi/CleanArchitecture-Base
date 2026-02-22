using System.Net.Http.Headers;
using Application.Abstractions.Data;
using Domain.Authorization;
using Domain.Users;
using Infrastructure.Database;
using Infrastructure.Files;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ArchitectureTests.Security;

public sealed class ApiWebApplicationFactory : WebApplicationFactory<Web.Api.Program>
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"arch-tests-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Notifications:Enabled"] = "false"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();
            services.RemoveAll<DbContextOptions<ApplicationReadDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ApplicationReadDbContext>>();
            services.RemoveAll<ApplicationDbContext>();
            services.RemoveAll<ApplicationReadDbContext>();
            services.RemoveAll<IApplicationDbContext>();
            services.RemoveAll<IApplicationReadDbContext>();
            services.RemoveAll<IUsersWriteDbContext>();
            services.RemoveAll<IUsersReadDbContext>();
            services.RemoveAll<IAuthorizationWriteDbContext>();
            services.RemoveAll<IAuthorizationReadDbContext>();
            services.RemoveAll<ITodosWriteDbContext>();
            services.RemoveAll<ITodosReadDbContext>();
            services.RemoveAll<IAuditWriteDbContext>();
            services.RemoveAll<IAuditReadDbContext>();
            services.RemoveAll<ILoggingWriteDbContext>();
            services.RemoveAll<ILoggingReadDbContext>();
            services.RemoveAll<IProfilesWriteDbContext>();
            services.RemoveAll<IProfilesReadDbContext>();
            services.RemoveAll<INotificationsWriteDbContext>();
            services.RemoveAll<INotificationsReadDbContext>();
            services.RemoveAll<IFilesWriteDbContext>();
            services.RemoveAll<IFilesReadDbContext>();
            services.RemoveAll<ISchedulerWriteDbContext>();
            services.RemoveAll<ISchedulerReadDbContext>();
            services.RemoveAll<IFileObjectStorage>();

            string connectionString = $"Data Source={_databasePath};Mode=ReadWriteCreate;Cache=Private;Pooling=False";

            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connectionString));
            services.AddDbContext<ApplicationReadDbContext>(options => options.UseSqlite(connectionString));
            services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
            services.AddScoped<IApplicationReadDbContext>(sp => sp.GetRequiredService<ApplicationReadDbContext>());
            services.AddScoped<IUsersWriteDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
            services.AddScoped<IUsersReadDbContext>(sp => sp.GetRequiredService<ApplicationReadDbContext>());
            services.AddScoped<IAuthorizationWriteDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
            services.AddScoped<IAuthorizationReadDbContext>(sp => sp.GetRequiredService<ApplicationReadDbContext>());
            services.AddScoped<ITodosWriteDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
            services.AddScoped<ITodosReadDbContext>(sp => sp.GetRequiredService<ApplicationReadDbContext>());
            services.AddScoped<IAuditWriteDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
            services.AddScoped<IAuditReadDbContext>(sp => sp.GetRequiredService<ApplicationReadDbContext>());
            services.AddScoped<ILoggingWriteDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
            services.AddScoped<ILoggingReadDbContext>(sp => sp.GetRequiredService<ApplicationReadDbContext>());
            services.AddScoped<IProfilesWriteDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
            services.AddScoped<IProfilesReadDbContext>(sp => sp.GetRequiredService<ApplicationReadDbContext>());
            services.AddScoped<INotificationsWriteDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
            services.AddScoped<INotificationsReadDbContext>(sp => sp.GetRequiredService<ApplicationReadDbContext>());
            services.AddScoped<IFilesWriteDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
            services.AddScoped<IFilesReadDbContext>(sp => sp.GetRequiredService<ApplicationReadDbContext>());
            services.AddScoped<ISchedulerWriteDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
            services.AddScoped<ISchedulerReadDbContext>(sp => sp.GetRequiredService<ApplicationReadDbContext>());
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
            try
            {
                if (File.Exists(_databasePath))
                {
                    File.Delete(_databasePath);
                }
            }
            catch
            {
                // Best effort cleanup for per-factory SQLite db file.
            }
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

        dbContext.Roles.AddRange(userRole, adminRole);
        var permissions = typeof(PermissionCodes)
            .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
            .Where(x => x.FieldType == typeof(string))
            .Select(x => (string?)x.GetValue(null))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Distinct(StringComparer.Ordinal)
            .Select(code => new Permission
            {
                Id = Guid.NewGuid(),
                Code = code!,
                Description = code!
            })
            .ToList();

        dbContext.Permissions.AddRange(permissions);

        Guid? todosReadPermissionId = permissions.SingleOrDefault(x => x.Code == PermissionCodes.TodosRead)?.Id;
        Guid? todosWritePermissionId = permissions.SingleOrDefault(x => x.Code == PermissionCodes.TodosWrite)?.Id;
        if (todosReadPermissionId.HasValue)
        {
            dbContext.RolePermissions.Add(new RolePermission { RoleId = userRole.Id, PermissionId = todosReadPermissionId.Value });
        }

        if (todosWritePermissionId.HasValue)
        {
            dbContext.RolePermissions.Add(new RolePermission { RoleId = userRole.Id, PermissionId = todosWritePermissionId.Value });
        }

        foreach (Permission permission in permissions)
        {
            dbContext.RolePermissions.Add(new RolePermission { RoleId = adminRole.Id, PermissionId = permission.Id });
        }

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
