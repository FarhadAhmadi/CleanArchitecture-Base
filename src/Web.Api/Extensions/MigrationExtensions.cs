using Infrastructure.Authorization;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Web.Api.Infrastructure;

namespace Web.Api.Extensions;

public static class MigrationExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder app, bool runAuthorizationSeed = true)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();

        using ApplicationDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        dbContext.Database.Migrate();

        if (!runAuthorizationSeed)
        {
            return;
        }

        AuthorizationSeeder seeder =
            scope.ServiceProvider.GetRequiredService<AuthorizationSeeder>();
        seeder.SeedAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    public static void ApplyMigrationsIfEnabled(this IApplicationBuilder app, DatabaseMigrationOptions options)
    {
        if (!options.ApplyOnStartup)
        {
            return;
        }

        app.ApplyMigrations(options.RunAuthorizationSeed);
    }
}
