using NetArchTest.Rules;
using Shouldly;

namespace ArchitectureTests.Modules;

public sealed class ModuleBoundariesTests : BaseTest
{
    [Fact]
    public void Profiles_Module_Should_Not_Depend_On_Logging_And_Notifications_Domain_Models()
    {
        TestResult result = Types.InAssembly(ApplicationAssembly)
            .That()
            .ResideInNamespace("Application.Profiles")
            .Should()
            .NotHaveDependencyOn("Domain.Logging")
            .And()
            .NotHaveDependencyOn("Domain.Notifications")
            .And()
            .NotHaveDependencyOn("Domain.Modules.Notifications")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }

    [Fact]
    public void Application_Module_Code_Should_Not_Depend_On_Infrastructure()
    {
        TestResult result = Types.InAssembly(ApplicationAssembly)
            .Should()
            .NotHaveDependencyOn("Infrastructure")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue();
    }
}
