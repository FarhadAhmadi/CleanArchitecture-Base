using System.Text.RegularExpressions;
using Shouldly;

namespace ArchitectureTests.Layers;

public sealed class WebApiThinLayerFitnessTests
{
    private static readonly string[] ApprovedLegacyOrchestrators =
    [
        Path.Combine("Endpoints", "Modules", "Files", "FileUseCaseService.cs"),
        Path.Combine("Endpoints", "Modules", "Notifications", "NotificationUseCaseService.cs")
    ];

    [Fact]
    public void WebApi_ShouldNotIntroduceNew_UseCaseService_Orchestrators()
    {
        string root = ResolveRepositoryRoot();
        string webApiRoot = Path.GetFullPath(Path.Combine(root, "src", "Web.Api"));
        string[] useCaseServices = Directory
            .EnumerateFiles(webApiRoot, "*UseCaseService.cs", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(webApiRoot, path))
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
        string modulesRoot = Path.GetFullPath(Path.Combine(root, "src", "Web.Api", "Endpoints", "Modules"));
        Regex suspiciousPatterns = new(@"\bwhile\s*\(|\bfor\s*\(|\bforeach\s*\(", RegexOptions.Compiled);
        string[] endpointFiles = Directory.EnumerateFiles(modulesRoot, "*.cs", SearchOption.AllDirectories).ToArray();

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
