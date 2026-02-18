using Application.Abstractions.Data;
using Infrastructure.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ContractTests;

public sealed class ApiContractWebApplicationFactory : WebApplicationFactory<Web.Api.Program>
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
            services.RemoveAll<IApplicationDbContext>();
            services.RemoveAll<IApplicationReadDbContext>();

            _connection.Open();

            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(_connection));
            services.AddDbContext<ApplicationReadDbContext>(options => options.UseSqlite(_connection));
            services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
            services.AddScoped<IApplicationReadDbContext>(sp => sp.GetRequiredService<ApplicationReadDbContext>());

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
            _connection.Dispose();
        }
    }
}
