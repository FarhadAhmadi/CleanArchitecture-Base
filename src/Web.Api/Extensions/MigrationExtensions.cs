using Infrastructure.Authorization;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Extensions;

public static class MigrationExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder app)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();

        using ApplicationDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        dbContext.Database.Migrate();

        AuthorizationSeeder seeder =
            scope.ServiceProvider.GetRequiredService<AuthorizationSeeder>();

        seeder.SeedAsync(CancellationToken.None).GetAwaiter().GetResult();
    }
}
