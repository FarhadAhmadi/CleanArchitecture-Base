using Shouldly;

namespace ArchitectureTests.Modules;

public sealed class ModuleStructureAndProjectLayoutTests
{
    private static readonly string[] LegacyDirectories =
    [
        Path.Combine("src", "Projects"),
        Path.Combine("src", "Application"),
        Path.Combine("src", "Domain"),
        Path.Combine("src", "SharedKernel"),
        Path.Combine("src", "Infrastructure"),
        Path.Combine("src", "Web.Api"),
        Path.Combine("src", "Application.csproj"),
        Path.Combine("src", "Domain.csproj"),
        Path.Combine("src", "SharedKernel.csproj")
    ];

    private static readonly (string Module, string[] Slices)[] ModuleSlices =
    [
        ("Audit", ["Api", "Application", "Domain", "Infrastructure"]),
        ("Authorization", ["Api", "Application", "Domain", "Infrastructure"]),
        ("Files", ["Api", "Application", "Domain", "Infrastructure"]),
        ("Logging", ["Api", "Application", "Domain", "Infrastructure"]),
        ("Notifications", ["Api", "Application", "Domain", "Infrastructure"]),
        ("Observability", ["Api", "Application"]),
        ("Profiles", ["Api", "Application", "Domain"]),
        ("Scheduler", ["Api", "Application", "Domain", "Infrastructure"]),
        ("Shared", ["Application"]),
        ("Todos", ["Api", "Application", "Domain"]),
        ("Users", ["Api", "Application", "Domain", "Infrastructure"])
    ];

    [Fact]
    public void Legacy_Project_Directories_Should_Not_Exist()
    {
        string root = ResolveRepositoryRoot();

        var existingLegacyPaths = LegacyDirectories
            .Select(path => Path.Combine(root, path))
            .Where(Directory.Exists)
            .Select(path => Path.GetRelativePath(root, path))
            .ToList();

        existingLegacyPaths.ShouldBeEmpty();
    }

    [Fact]
    public void Core_Project_Files_Should_Exist_Under_Core_Directory()
    {
        string root = ResolveRepositoryRoot();

        File.Exists(Path.Combine(root, "src", "Core", "Application", "Application.csproj")).ShouldBeTrue();
        File.Exists(Path.Combine(root, "src", "Core", "Domain", "Domain.csproj")).ShouldBeTrue();
        File.Exists(Path.Combine(root, "src", "Core", "Kernel", "SharedKernel.csproj")).ShouldBeTrue();
        File.Exists(Path.Combine(root, "src", "Platform", "Infrastructure.csproj")).ShouldBeTrue();
        File.Exists(Path.Combine(root, "src", "Host", "Web.Api.csproj")).ShouldBeTrue();
    }

    [Fact]
    public void Modules_Should_Follow_Expected_Slice_Layout()
    {
        string root = ResolveRepositoryRoot();
        List<string> missingSlices = [];

        foreach ((string module, string[] slices) in ModuleSlices)
        {
            foreach (string slice in slices)
            {
                string slicePath = Path.Combine(root, "src", "Modules", module, slice);
                if (!Directory.Exists(slicePath))
                {
                    missingSlices.Add(Path.GetRelativePath(root, slicePath));
                }
            }
        }

        missingSlices.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("Audit")]
    [InlineData("Authorization")]
    [InlineData("Files")]
    [InlineData("Logging")]
    [InlineData("Notifications")]
    [InlineData("Observability")]
    [InlineData("Profiles")]
    [InlineData("Scheduler")]
    [InlineData("Todos")]
    [InlineData("Users")]
    public void Api_Modules_Should_Have_At_Least_One_Endpoint_File(string module)
    {
        string root = ResolveRepositoryRoot();
        string apiPath = Path.Combine(root, "src", "Modules", module, "Api");

        Directory.Exists(apiPath).ShouldBeTrue();

        string[] endpointFiles = Directory.GetFiles(apiPath, "*.cs", SearchOption.AllDirectories);
        endpointFiles.Length.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Infrastructure_DependencyInjection_Should_Register_All_Platform_Modules()
    {
        string root = ResolveRepositoryRoot();
        string dependencyInjectionPath = Path.Combine(root, "src", "Shared", "Infrastructure", "DependencyInjection.cs");
        string content = File.ReadAllText(dependencyInjectionPath);

        string[] registrations =
        [
            ".AddCoreModule()",
            ".AddDataAccessModule(configuration)",
            ".AddCachingModule(configuration)",
            ".AddIntegrationModule(configuration)",
            ".AddHealthChecksModule(configuration)",
            ".AddLoggingModule(configuration)",
            ".AddUsersModule(configuration)",
            ".AddAuthModule(configuration)",
            ".AddAuthorizationModule(configuration)",
            ".AddAuditModule()",
            ".AddMonitoringModule()",
            ".AddFilesModule(configuration)",
            ".AddNotificationsModule(configuration)",
            ".AddSchedulerModule(configuration)"
        ];

        var missingRegistrations = registrations
            .Where(registration => !content.Contains(registration, StringComparison.Ordinal))
            .ToList();

        missingRegistrations.ShouldBeEmpty();
    }

    [Fact]
    public void Refactored_Application_Areas_Should_Not_Have_Flat_Files_In_Root()
    {
        string root = ResolveRepositoryRoot();

        string[] refactoredRoots =
        [
            Path.Combine(root, "src", "Modules", "Authorization", "Application", "Roles"),
            Path.Combine(root, "src", "Modules", "Files", "Application"),
            Path.Combine(root, "src", "Modules", "Logging", "Application"),
            Path.Combine(root, "src", "Modules", "Notifications", "Application"),
            Path.Combine(root, "src", "Modules", "Scheduler", "Application"),
            Path.Combine(root, "src", "Modules", "Profiles", "Application")
        ];

        List<string> offenders = [];

        foreach (string area in refactoredRoots)
        {
            foreach (string file in Directory.GetFiles(area, "*.cs", SearchOption.TopDirectoryOnly))
            {
                offenders.Add(Path.GetRelativePath(root, file));
            }
        }

        offenders.ShouldBeEmpty();
    }

    [Fact]
    public void Refactored_Application_Areas_Should_Contain_Structure_Folders()
    {
        string root = ResolveRepositoryRoot();

        string[] expectedDirectories =
        [
            Path.Combine(root, "src", "Modules", "Authorization", "Application", "Roles", "Commands"),
            Path.Combine(root, "src", "Modules", "Authorization", "Application", "Roles", "Queries"),
            Path.Combine(root, "src", "Modules", "Files", "Application", "Commands"),
            Path.Combine(root, "src", "Modules", "Files", "Application", "Queries"),
            Path.Combine(root, "src", "Modules", "Logging", "Application", "Commands"),
            Path.Combine(root, "src", "Modules", "Logging", "Application", "Queries"),
            Path.Combine(root, "src", "Modules", "Notifications", "Application", "Commands"),
            Path.Combine(root, "src", "Modules", "Notifications", "Application", "Queries"),
            Path.Combine(root, "src", "Modules", "Scheduler", "Application", "Commands"),
            Path.Combine(root, "src", "Modules", "Scheduler", "Application", "Queries"),
            Path.Combine(root, "src", "Modules", "Profiles", "Application", "Commands"),
            Path.Combine(root, "src", "Modules", "Profiles", "Application", "Queries")
        ];

        var missingDirectories = expectedDirectories
            .Where(path => !Directory.Exists(path))
            .Select(path => Path.GetRelativePath(root, path))
            .ToList();

        missingDirectories.ShouldBeEmpty();
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

