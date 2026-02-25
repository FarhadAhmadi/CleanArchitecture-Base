using Shouldly;

namespace ArchitectureTests.Modules;

public sealed class DbContextIsolationTests
{
    [Fact]
    public void Application_And_Api_Slices_Should_Not_Use_Global_DbContext_Directly()
    {
        string root = ResolveRepositoryRoot();
        string modulesRoot = Path.Combine(root, "src", "Modules");

        string[] layers =
        [
            $"{Path.DirectorySeparatorChar}Application{Path.DirectorySeparatorChar}",
            $"{Path.DirectorySeparatorChar}Api{Path.DirectorySeparatorChar}"
        ];

        List<string> offenders = [];

        foreach (string file in Directory.EnumerateFiles(modulesRoot, "*.cs", SearchOption.AllDirectories))
        {
            if (!layers.Any(layer => file.Contains(layer, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            string content = File.ReadAllText(file);
            if (content.Contains("ApplicationDbContext", StringComparison.Ordinal) ||
                content.Contains("ApplicationReadDbContext", StringComparison.Ordinal))
            {
                offenders.Add(Path.GetRelativePath(root, file));
            }
        }

        offenders.ShouldBeEmpty("Use module DbContext ports in Api/Application layers; avoid direct global DbContext usage.");
    }

    private static string ResolveRepositoryRoot()
    {
        string current = AppContext.BaseDirectory;

        while (!string.IsNullOrWhiteSpace(current))
        {
            if (File.Exists(Path.Combine(current, "CleanArchitecture.slnx")))
            {
                return current;
            }

            DirectoryInfo? parent = Directory.GetParent(current);
            if (parent is null)
            {
                break;
            }

            current = parent.FullName;
        }

        throw new InvalidOperationException("Cannot resolve repository root from test execution directory.");
    }
}
