using Application.Abstractions.Validation;
using FluentValidation;

namespace Application.Authorization.AssignRoleToUser;

internal sealed class AssignRoleToUserCommandValidator : AbstractValidator<AssignRoleToUserCommand>
{
    public AssignRoleToUserCommandValidator()
    {
        RuleFor(c => c.UserId).NotEmptyGuid();
        RuleFor(c => c.RoleName).NotEmpty().MaximumLength(100);
    }
}
