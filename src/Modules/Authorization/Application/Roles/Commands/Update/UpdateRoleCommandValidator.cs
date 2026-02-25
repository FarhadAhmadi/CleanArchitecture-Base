using Application.Abstractions.Validation;
using FluentValidation;

namespace Application.Authorization.Roles;

internal sealed class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleCommandValidator()
    {
        RuleFor(c => c.RoleId).NotEmptyGuid();
        RuleFor(c => c.RoleName).NotEmpty().MaximumLength(100);
    }
}
