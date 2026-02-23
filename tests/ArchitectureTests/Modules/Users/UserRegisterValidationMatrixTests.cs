using Application;
using Application.Users.Register;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace ArchitectureTests.Modules.Users;

public sealed class UserRegisterValidationMatrixTests
{
    [Theory]
    [MemberData(nameof(RegisterCases))]
    public async Task RegisterUserCommandValidator_ShouldHandleMatrix(
        string firstName,
        string lastName,
        string email,
        string password)
    {
        ServiceProvider services = BuildServices();
        IValidator<RegisterUserCommand> validator = services.GetRequiredService<IValidator<RegisterUserCommand>>();

        RegisterUserCommand command = new(email, firstName, lastName, password);
        FluentValidation.Results.ValidationResult result = await validator.ValidateAsync(command);

        bool expectedValid =
            !string.IsNullOrWhiteSpace(firstName) &&
            !string.IsNullOrWhiteSpace(lastName) &&
            IsValidEmail(email) &&
            IsStrongPassword(password);

        result.IsValid.ShouldBe(expectedValid);
    }

    public static IEnumerable<object[]> RegisterCases()
    {
        string[] emails =
        [
            "",
            "   ",
            "invalid",
            "foo@",
            "@bar.com",
            "foo@bar",
            "foo@bar.",
            "foo@bar.com",
            "user.name+tag@example.co",
            "a@b.co"
        ];

        string[] passwords =
        [
            "",
            " ",
            "short",
            "1234567",
            "12345678",
            "Password1",
            "P@ssw0rd!",
            "longpassword123",
            "abcdEFGH",
            "very-very-strong-pass"
        ];

        foreach (string email in emails)
        {
            foreach (string password in passwords)
            {
                yield return ["John", "Doe", email, password];
            }
        }
    }

    private static ServiceProvider BuildServices()
    {
        return new ServiceCollection()
            .AddApplication()
            .BuildServiceProvider();
    }

    private static bool IsValidEmail(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        try
        {
            var _ = new System.Net.Mail.MailAddress(value);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static bool IsStrongPassword(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length < 12)
        {
            return false;
        }

        bool hasUpper = value.Any(char.IsUpper);
        bool hasLower = value.Any(char.IsLower);
        bool hasDigit = value.Any(char.IsDigit);
        bool hasSpecial = value.Any(ch => !char.IsLetterOrDigit(ch));

        return hasUpper && hasLower && hasDigit && hasSpecial;
    }
}
