using Infrastructure.Integration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure.Database;

internal sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        DbContextOptionsBuilder<ApplicationDbContext> builder = new();

        string connectionString =
            Environment.GetEnvironmentVariable("ConnectionStrings__Database") ??
            "Server=(localdb)\\mssqllocaldb;Database=CleanArchitectureBase;Trusted_Connection=True;TrustServerCertificate=True;";

        builder.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
            sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", Schemas.Default);
        });

        return new ApplicationDbContext(builder.Options, new IntegrationEventSerializer());
    }
}
