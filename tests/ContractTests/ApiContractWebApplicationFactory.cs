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
