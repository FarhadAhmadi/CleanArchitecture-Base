using Application;
using Application.Profiles;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ArchitectureTests.Modules.Profiles;

public sealed class ProfileValidationUnitTests
{
    [Fact]
    public async Task CreateMyProfileCommandValidator_ShouldRejectEmptyDisplayName()
    {
        ServiceProvider services = BuildServices();
        IValidator<CreateMyProfileCommand> validator = services.GetRequiredService<IValidator<CreateMyProfileCommand>>();

        CreateMyProfileCommand command = new("", "en-US", true);
        FluentValidation.Results.ValidationResult result = await validator.ValidateAsync(command);

        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public async Task UpdateMyProfileBasicCommandValidator_ShouldRejectFutureDateOfBirth()
    {
        ServiceProvider services = BuildServices();
        IValidator<UpdateMyProfileBasicCommand> validator = services.GetRequiredService<IValidator<UpdateMyProfileBasicCommand>>();

        UpdateMyProfileBasicCommand command = new("name", "bio", DateTime.UtcNow.AddDays(1), "n/a", "test");
        FluentValidation.Results.ValidationResult result = await validator.ValidateAsync(command);

        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public async Task GetProfilesAdminReportQueryValidator_ShouldRejectInvalidRange()
    {
        ServiceProvider services = BuildServices();
        IValidator<GetProfilesAdminReportQuery> validator = services.GetRequiredService<IValidator<GetProfilesAdminReportQuery>>();

        GetProfilesAdminReportQuery query = new(
            Page: 1,
            PageIndex: null,
            PageSize: 10,
            Search: null,
            IsProfilePublic: null,
            PreferredLanguage: null,
            MinCompleteness: 90,
            MaxCompleteness: 10,
            UpdatedFrom: DateTime.UtcNow,
            UpdatedTo: DateTime.UtcNow.AddDays(-1));

        FluentValidation.Results.ValidationResult result = await validator.ValidateAsync(query);

        result.IsValid.ShouldBeFalse();
        result.Errors.Count.ShouldBeGreaterThanOrEqualTo(2);
    }

    private static ServiceProvider BuildServices()
    {
        return new ServiceCollection()
            .AddApplication()
            .BuildServiceProvider();
    }
}
