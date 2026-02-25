using FluentValidation;

namespace Application.Authorization.Roles;

internal sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(c => c.RoleName).NotEmpty().MaximumLength(100);
    }
}
