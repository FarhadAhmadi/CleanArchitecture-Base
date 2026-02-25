using Application.Abstractions.Data;
using Infrastructure.Database;
using Infrastructure.Seeding;
using Infrastructure.Integration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

internal static class DataAccessModule
{
    private static Action<DbContextOptionsBuilder> BuildDbOptions(
        string? connectionString,
        string historyTable)
    {
        return options =>
        {
            if (!string.IsNullOrWhiteSpace(connectionString) &&
                (connectionString.Contains("Data Source=", StringComparison.OrdinalIgnoreCase) ||
                 connectionString.Contains("DataSource=", StringComparison.OrdinalIgnoreCase)))
            {
                options.UseSqlite(connectionString);
                return;
            }

            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsHistoryTable(historyTable, Schemas.Default);
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(15),
                    errorNumbersToAdd: null);
            });
        };
    }

    private static string ResolveModuleConnectionString(IConfiguration configuration, string moduleName)
    {
        return configuration.GetConnectionString(moduleName)
               ?? configuration.GetConnectionString("Database")
               ?? throw new InvalidOperationException($"Connection string not found for '{moduleName}' or fallback 'Database'.");
    }

    internal static IServiceCollection AddDataAccessModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string usersConnectionString = ResolveModuleConnectionString(configuration, "Users");
        string authorizationConnectionString = ResolveModuleConnectionString(configuration, "Authorization");
        string todosConnectionString = ResolveModuleConnectionString(configuration, "Todos");
        string auditConnectionString = ResolveModuleConnectionString(configuration, "Audit");
        string loggingConnectionString = ResolveModuleConnectionString(configuration, "Logging");
        string profilesConnectionString = ResolveModuleConnectionString(configuration, "Profiles");
        string notificationsConnectionString = ResolveModuleConnectionString(configuration, "Notifications");
        string filesConnectionString = ResolveModuleConnectionString(configuration, "Files");
        string schedulerConnectionString = ResolveModuleConnectionString(configuration, "Scheduler");
        string platformConnectionString = ResolveModuleConnectionString(configuration, "Database");

        services.AddSingleton<IntegrationEventSerializer>();

        services.AddDbContext<ApplicationDbContext>(BuildDbOptions(platformConnectionString, HistoryRepository.DefaultTableName));
        services.AddDbContext<ApplicationReadDbContext>(BuildDbOptions(platformConnectionString, HistoryRepository.DefaultTableName));

        services.AddDbContext<UsersWriteDbContext>(BuildDbOptions(usersConnectionString, "__EFMigrationsHistory_Users"));
        services.AddDbContext<UsersReadDbContext>(BuildDbOptions(usersConnectionString, "__EFMigrationsHistory_Users"));
        services.AddDbContext<AuthorizationWriteDbContext>(BuildDbOptions(authorizationConnectionString, "__EFMigrationsHistory_Authorization"));
        services.AddDbContext<AuthorizationReadDbContext>(BuildDbOptions(authorizationConnectionString, "__EFMigrationsHistory_Authorization"));
        services.AddDbContext<TodosWriteDbContext>(BuildDbOptions(todosConnectionString, "__EFMigrationsHistory_Todos"));
        services.AddDbContext<TodosReadDbContext>(BuildDbOptions(todosConnectionString, "__EFMigrationsHistory_Todos"));
        services.AddDbContext<AuditWriteDbContext>(BuildDbOptions(auditConnectionString, "__EFMigrationsHistory_Audit"));
        services.AddDbContext<AuditReadDbContext>(BuildDbOptions(auditConnectionString, "__EFMigrationsHistory_Audit"));
        services.AddDbContext<LoggingWriteDbContext>(BuildDbOptions(loggingConnectionString, "__EFMigrationsHistory_Logging"));
        services.AddDbContext<LoggingReadDbContext>(BuildDbOptions(loggingConnectionString, "__EFMigrationsHistory_Logging"));
        services.AddDbContext<ProfilesWriteDbContext>(BuildDbOptions(profilesConnectionString, "__EFMigrationsHistory_Profiles"));
        services.AddDbContext<ProfilesReadDbContext>(BuildDbOptions(profilesConnectionString, "__EFMigrationsHistory_Profiles"));
        services.AddDbContext<NotificationsWriteDbContext>(BuildDbOptions(notificationsConnectionString, "__EFMigrationsHistory_Notifications"));
        services.AddDbContext<NotificationsReadDbContext>(BuildDbOptions(notificationsConnectionString, "__EFMigrationsHistory_Notifications"));
        services.AddDbContext<FilesWriteDbContext>(BuildDbOptions(filesConnectionString, "__EFMigrationsHistory_Files"));
        services.AddDbContext<FilesReadDbContext>(BuildDbOptions(filesConnectionString, "__EFMigrationsHistory_Files"));
        services.AddDbContext<SchedulerWriteDbContext>(BuildDbOptions(schedulerConnectionString, "__EFMigrationsHistory_Scheduler"));
        services.AddDbContext<SchedulerReadDbContext>(BuildDbOptions(schedulerConnectionString, "__EFMigrationsHistory_Scheduler"));

        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IApplicationReadDbContext>(sp =>
            sp.GetRequiredService<ApplicationReadDbContext>());

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
        services.AddScoped<SampleDataSeeder>();

        return services;
    }
}
