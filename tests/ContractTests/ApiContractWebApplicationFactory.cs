using Application.Abstractions.Data;
using Infrastructure.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ContractTests;

public sealed class ApiContractWebApplicationFactory : WebApplicationFactory<Web.Api.Program>
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"contract-tests-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

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

            using IServiceScope scope = services.BuildServiceProvider().CreateScope();
            ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.EnsureCreated();
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
}
