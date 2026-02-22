using Shouldly;

namespace ArchitectureTests.Modules;

public sealed class CrossModuleWriteGuardsTests
{
    [Fact]
    public void NotificationMessages_Should_Be_Written_Only_By_Allowed_Writers()
    {
        string root = ResolveRepositoryRoot();
        string srcRoot = Path.Combine(root, "src");

        string[] allowedSuffixes =
        [
            Path.Combine("Web.Api", "Endpoints", "Modules", "Notifications", "NotificationUseCaseService.cs"),
            Path.Combine("Infrastructure", "Modules", "Notifications", "Infrastructure", "Notifications", "NotificationMessageWriter.cs"),
            Path.Combine("Infrastructure", "Database", "Seeding", "SampleDataSeeder.cs")
        ];

        var offenders = FindDirectAddCalls(srcRoot, "NotificationMessages.Add(")
            .Where(path => !allowedSuffixes.Any(suffix => path.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        offenders.ShouldBeEmpty("Direct NotificationMessages writes outside allowed writers are forbidden.");
    }

    [Fact]
    public void LogEvents_Should_Be_Written_Only_By_Allowed_Writers()
    {
        string root = ResolveRepositoryRoot();
        string srcRoot = Path.Combine(root, "src");

        string[] allowedSuffixes =
        [
            Path.Combine("Infrastructure", "Modules", "Logging", "Infrastructure", "Logging", "ApplicationLogWriter.cs"),
            Path.Combine("Infrastructure", "Modules", "Logging", "Infrastructure", "Logging", "LogIngestionService.cs"),
            Path.Combine("Infrastructure", "Modules", "Logging", "Infrastructure", "Logging", "LogRetryWorker.cs"),
            Path.Combine("Infrastructure", "Database", "Seeding", "SampleDataSeeder.cs")
        ];

        var offenders = FindDirectAddCalls(srcRoot, "LogEvents.Add(")
            .Where(path => !allowedSuffixes.Any(suffix => path.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)))
            .ToList();

        offenders.ShouldBeEmpty("Direct LogEvents writes outside allowed writers are forbidden.");
    }

    private static IEnumerable<string> FindDirectAddCalls(string srcRoot, string pattern)
    {
        foreach (string file in Directory.EnumerateFiles(srcRoot, "*.cs", SearchOption.AllDirectories))
        {
            string content = File.ReadAllText(file);
            if (content.Contains(pattern, StringComparison.Ordinal))
            {
                yield return file;
            }
        }
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
