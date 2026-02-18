using System.Text.Json;

namespace ContractTests;

public sealed class OpenApiContractTests : IClassFixture<ApiContractWebApplicationFactory>
{
    private const string SnapshotFileName = "openapi.v1.snapshot.json";
    private static readonly JsonSerializerOptions SnapshotJsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly ApiContractWebApplicationFactory _factory;

    public OpenApiContractTests(ApiContractWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task SwaggerV1_ShouldMatchApprovedContract()
    {
        using HttpClient client = _factory.CreateClient();
        string current = await client.GetStringAsync("/swagger/v1/swagger.json");
        string normalized = NormalizeJson(current);

        string snapshotPath = GetSnapshotPath();
        bool updateSnapshot = string.Equals(
            Environment.GetEnvironmentVariable("UPDATE_API_CONTRACT"),
            "true",
            StringComparison.OrdinalIgnoreCase);

        if (updateSnapshot)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(snapshotPath)!);
            await File.WriteAllTextAsync(snapshotPath, normalized);
        }

        Assert.True(File.Exists(snapshotPath), $"Snapshot not found: {snapshotPath}. Set UPDATE_API_CONTRACT=true to generate it.");

        string approved = await File.ReadAllTextAsync(snapshotPath);
        Assert.Equal(approved, normalized);
    }

    private static string GetSnapshotPath()
    {
        string baseDir = AppContext.BaseDirectory;
        string projectDir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", ".."));
        return Path.Combine(projectDir, "Snapshots", SnapshotFileName);
    }

    private static string NormalizeJson(string rawJson)
    {
        using var document = JsonDocument.Parse(rawJson);
        return JsonSerializer.Serialize(document, SnapshotJsonOptions);
    }
}
