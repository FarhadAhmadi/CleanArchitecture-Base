using Infrastructure.Authorization;
using Infrastructure.Database;
using Infrastructure.Notifications;
using Microsoft.EntityFrameworkCore;
using Web.Api.Infrastructure;

namespace Web.Api.Extensions;

public static class MigrationExtensions
{
    public static async Task ApplyMigrationsAsync(this IApplicationBuilder app, bool runAuthorizationSeed = true, CancellationToken cancellationToken = default)
    {
        using IServiceScope scope = app.ApplicationServices.CreateScope();

        using ApplicationDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await dbContext.Database.MigrateAsync(cancellationToken);

        if (runAuthorizationSeed)
        {
            AuthorizationSeeder seeder =
                scope.ServiceProvider.GetRequiredService<AuthorizationSeeder>();
            await seeder.SeedAsync(cancellationToken);
        }

        NotificationTemplateSeeder notificationTemplateSeeder =
            scope.ServiceProvider.GetRequiredService<NotificationTemplateSeeder>();
        await notificationTemplateSeeder.SeedAsync(cancellationToken);
    }

    public static async Task ApplyMigrationsIfEnabledAsync(
        this IApplicationBuilder app,
        DatabaseMigrationOptions options,
        CancellationToken cancellationToken = default)
    {
        if (!options.ApplyOnStartup)
        {
            return;
        }

        await app.ApplyMigrationsAsync(options.RunAuthorizationSeed, cancellationToken);
    }
}

