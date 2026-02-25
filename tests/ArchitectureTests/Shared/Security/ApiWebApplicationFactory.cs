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
                ["Notifications:Enabled"] = "false",
                ["FileCleanup:Enabled"] = "false"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();
            services.RemoveAll<DbContextOptions<ApplicationReadDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ApplicationReadDbContext>>();
            services.RemoveAll<DbContextOptions<UsersWriteDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<UsersWriteDbContext>>();
            services.RemoveAll<DbContextOptions<UsersReadDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<UsersReadDbContext>>();
            services.RemoveAll<DbContextOptions<AuthorizationWriteDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AuthorizationWriteDbContext>>();
            services.RemoveAll<DbContextOptions<AuthorizationReadDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AuthorizationReadDbContext>>();
            services.RemoveAll<DbContextOptions<TodosWriteDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<TodosWriteDbContext>>();
            services.RemoveAll<DbContextOptions<TodosReadDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<TodosReadDbContext>>();
            services.RemoveAll<DbContextOptions<AuditWriteDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AuditWriteDbContext>>();
            services.RemoveAll<DbContextOptions<AuditReadDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AuditReadDbContext>>();
            services.RemoveAll<DbContextOptions<LoggingWriteDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<LoggingWriteDbContext>>();
            services.RemoveAll<DbContextOptions<LoggingReadDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<LoggingReadDbContext>>();
            services.RemoveAll<DbContextOptions<ProfilesWriteDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ProfilesWriteDbContext>>();
            services.RemoveAll<DbContextOptions<ProfilesReadDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<ProfilesReadDbContext>>();
            services.RemoveAll<DbContextOptions<NotificationsWriteDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<NotificationsWriteDbContext>>();
            services.RemoveAll<DbContextOptions<NotificationsReadDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<NotificationsReadDbContext>>();
            services.RemoveAll<DbContextOptions<FilesWriteDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<FilesWriteDbContext>>();
            services.RemoveAll<DbContextOptions<FilesReadDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<FilesReadDbContext>>();
            services.RemoveAll<DbContextOptions<SchedulerWriteDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<SchedulerWriteDbContext>>();
            services.RemoveAll<DbContextOptions<SchedulerReadDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<SchedulerReadDbContext>>();
            services.RemoveAll<ApplicationDbContext>();
            services.RemoveAll<ApplicationReadDbContext>();
            services.RemoveAll<UsersWriteDbContext>();
            services.RemoveAll<UsersReadDbContext>();
            services.RemoveAll<AuthorizationWriteDbContext>();
            services.RemoveAll<AuthorizationReadDbContext>();
            services.RemoveAll<TodosWriteDbContext>();
            services.RemoveAll<TodosReadDbContext>();
            services.RemoveAll<AuditWriteDbContext>();
            services.RemoveAll<AuditReadDbContext>();
            services.RemoveAll<LoggingWriteDbContext>();
            services.RemoveAll<LoggingReadDbContext>();
            services.RemoveAll<ProfilesWriteDbContext>();
            services.RemoveAll<ProfilesReadDbContext>();
            services.RemoveAll<NotificationsWriteDbContext>();
            services.RemoveAll<NotificationsReadDbContext>();
            services.RemoveAll<FilesWriteDbContext>();
            services.RemoveAll<FilesReadDbContext>();
            services.RemoveAll<SchedulerWriteDbContext>();
            services.RemoveAll<SchedulerReadDbContext>();
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
            services.AddDbContext<UsersWriteDbContext>(options => options.UseSqlite(connectionString));
            services.AddDbContext<UsersReadDbContext>(options => options.UseSqlite(connectionString));
            services.AddDbContext<AuthorizationWriteDbContext>(options => options.UseSqlite(connectionString));
            services.AddDbContext<AuthorizationReadDbContext>(options => options.UseSqlite(connectionString));
            services.AddDbContext<TodosWriteDbContext>(options => options.UseSqlite(connectionString));
            services.AddDbContext<TodosReadDbContext>(options => options.UseSqlite(connectionString));
            services.AddDbContext<AuditWriteDbContext>(options => options.UseSqlite(connectionString));
            services.AddDbContext<AuditReadDbContext>(options => options.UseSqlite(connectionString));
            services.AddDbContext<LoggingWriteDbContext>(options => options.UseSqlite(connectionString));
            services.AddDbContext<LoggingReadDbContext>(options => options.UseSqlite(connectionString));
            services.AddDbContext<ProfilesWriteDbContext>(options => options.UseSqlite(connectionString));
            services.AddDbContext<ProfilesReadDbContext>(options => options.UseSqlite(connectionString));
            services.AddDbContext<NotificationsWriteDbContext>(options => options.UseSqlite(connectionString));
            services.AddDbContext<NotificationsReadDbContext>(options => options.UseSqlite(connectionString));
            services.AddDbContext<FilesWriteDbContext>(options => options.UseSqlite(connectionString));
            services.AddDbContext<FilesReadDbContext>(options => options.UseSqlite(connectionString));
            services.AddDbContext<SchedulerWriteDbContext>(options => options.UseSqlite(connectionString));
            services.AddDbContext<SchedulerReadDbContext>(options => options.UseSqlite(connectionString));
            services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
            services.AddScoped<IApplicationReadDbContext>(sp => sp.GetRequiredService<ApplicationReadDbContext>());
            services.AddScoped<IUsersWriteDbContext>(sp => sp.GetRequiredService<UsersWriteDbContext>());
            services.AddScoped<IUsersReadDbContext>(sp => sp.GetRequiredService<UsersReadDbContext>());
            services.AddScoped<IAuthorizationWriteDbContext>(sp => sp.GetRequiredService<AuthorizationWriteDbContext>());
            services.AddScoped<IAuthorizationReadDbContext>(sp => sp.GetRequiredService<AuthorizationReadDbContext>());
            services.AddScoped<ITodosWriteDbContext>(sp => sp.GetRequiredService<TodosWriteDbContext>());
            services.AddScoped<ITodosReadDbContext>(sp => sp.GetRequiredService<TodosReadDbContext>());
            services.AddScoped<IAuditWriteDbContext>(sp => sp.GetRequiredService<AuditWriteDbContext>());
            services.AddScoped<IAuditReadDbContext>(sp => sp.GetRequiredService<AuditReadDbContext>());
            services.AddScoped<ILoggingWriteDbContext>(sp => sp.GetRequiredService<LoggingWriteDbContext>());
            services.AddScoped<ILoggingReadDbContext>(sp => sp.GetRequiredService<LoggingReadDbContext>());
            services.AddScoped<IProfilesWriteDbContext>(sp => sp.GetRequiredService<ProfilesWriteDbContext>());
            services.AddScoped<IProfilesReadDbContext>(sp => sp.GetRequiredService<ProfilesReadDbContext>());
            services.AddScoped<INotificationsWriteDbContext>(sp => sp.GetRequiredService<NotificationsWriteDbContext>());
            services.AddScoped<INotificationsReadDbContext>(sp => sp.GetRequiredService<NotificationsReadDbContext>());
            services.AddScoped<IFilesWriteDbContext>(sp => sp.GetRequiredService<FilesWriteDbContext>());
            services.AddScoped<IFilesReadDbContext>(sp => sp.GetRequiredService<FilesReadDbContext>());
            services.AddScoped<ISchedulerWriteDbContext>(sp => sp.GetRequiredService<SchedulerWriteDbContext>());
            services.AddScoped<ISchedulerReadDbContext>(sp => sp.GetRequiredService<SchedulerReadDbContext>());
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

        public Task<bool> ExistsAsync(string objectKey, CancellationToken cancellationToken)
        {
            _ = objectKey;
            _ = cancellationToken;
            return Task.FromResult(true);
        }

        public Task MarkHealthyAsync(CancellationToken cancellationToken)
        {
            _ = cancellationToken;
            return Task.CompletedTask;
        }
    }
}
