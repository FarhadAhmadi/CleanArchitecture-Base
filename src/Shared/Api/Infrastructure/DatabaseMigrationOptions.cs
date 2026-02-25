namespace Web.Api.Infrastructure;

public sealed class DatabaseMigrationOptions
{
    public const string SectionName = "DatabaseMigrations";

    public bool ApplyOnStartup { get; init; }
    public bool RunAuthorizationSeed { get; init; } = true;
    public bool RunSampleDataSeed { get; init; }
}
