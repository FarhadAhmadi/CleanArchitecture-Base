using Application.Abstractions.Validation;
using FluentValidation;

namespace Application.Users.Management;

internal sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(c => c.Email).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(c => c.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(c => c.LastName).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Password).StrongPassword();
        RuleFor(c => c.PhoneNumber).MaximumLength(32).When(c => !string.IsNullOrWhiteSpace(c.PhoneNumber));
        RuleFor(c => c.FailedLoginCount).GreaterThanOrEqualTo(0).When(c => c.FailedLoginCount.HasValue);
    }
}
