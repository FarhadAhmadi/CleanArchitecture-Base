using System.Text.RegularExpressions;
using Shouldly;

namespace ArchitectureTests.Layers;

public sealed class WebApiThinLayerFitnessTests
{
    private static readonly string[] ApprovedLegacyOrchestrators =
    [
        Path.Combine("Modules", "Files", "Api", "Endpoints", "FileUseCaseService.cs"),
        Path.Combine("Modules", "Notifications", "Api", "Endpoints", "NotificationUseCaseService.cs")
    ];

    [Fact]
    public void WebApi_ShouldNotIntroduceNew_UseCaseService_Orchestrators()
    {
        string root = ResolveRepositoryRoot();
        string srcRoot = Path.GetFullPath(Path.Combine(root, "src"));
        string[] useCaseServices = Directory
            .EnumerateFiles(srcRoot, "*UseCaseService.cs", SearchOption.AllDirectories)
            .Where(path => path.Contains($"{Path.DirectorySeparatorChar}Api{Path.DirectorySeparatorChar}Endpoints{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .Select(path => Path.GetRelativePath(srcRoot, path))
            .ToArray();

        foreach (string service in useCaseServices)
        {
            bool approved = ApprovedLegacyOrchestrators.Any(x => string.Equals(x, service, StringComparison.OrdinalIgnoreCase));
            approved.ShouldBeTrue($"Unexpected orchestrator in Web.Api: {service}");
        }
    }

    [Fact]
    public void WebApi_Endpoints_ShouldNotContainHeavyBusinessLoops()
    {
        string root = ResolveRepositoryRoot();
        string modulesRoot = Path.GetFullPath(Path.Combine(root, "src", "Modules"));
        Regex suspiciousPatterns = new(@"\bwhile\s*\(|\bfor\s*\(|\bforeach\s*\(", RegexOptions.Compiled);
        string[] endpointFiles = Directory
            .EnumerateFiles(modulesRoot, "*.cs", SearchOption.AllDirectories)
            .Where(path => path.Contains($"{Path.DirectorySeparatorChar}Api{Path.DirectorySeparatorChar}Endpoints{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        List<string> offenders = [];

        foreach (string file in endpointFiles)
        {
            string fileName = Path.GetFileName(file);
            if (fileName.EndsWith("UseCaseService.cs", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string content = File.ReadAllText(file);
            if (suspiciousPatterns.IsMatch(content))
            {
                offenders.Add(Path.GetRelativePath(root, file));
            }
        }

        offenders.ShouldBeEmpty("Endpoint layer should remain orchestration-light. Move loops/business flow to Application layer.");
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
