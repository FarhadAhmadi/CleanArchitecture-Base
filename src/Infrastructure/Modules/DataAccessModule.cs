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
    internal static IServiceCollection AddDataAccessModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("Database");

        services.AddSingleton<IntegrationEventSerializer>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsHistoryTable(
                    HistoryRepository.DefaultTableName,
                    Schemas.Default);
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(15),
                    errorNumbersToAdd: null);
            }));

        services.AddDbContext<ApplicationReadDbContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsHistoryTable(
                    HistoryRepository.DefaultTableName,
                    Schemas.Default);
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(15),
                    errorNumbersToAdd: null);
            }));

        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<IApplicationReadDbContext>(sp =>
            sp.GetRequiredService<ApplicationReadDbContext>());
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
        services.AddScoped<SampleDataSeeder>();

        return services;
    }
}
