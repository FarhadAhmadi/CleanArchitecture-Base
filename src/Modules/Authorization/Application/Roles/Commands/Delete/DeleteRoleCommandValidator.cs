using Application.Abstractions.Validation;
using FluentValidation;

namespace Application.Authorization.Roles;

internal sealed class DeleteRoleCommandValidator : AbstractValidator<DeleteRoleCommand>
{
    public DeleteRoleCommandValidator()
    {
        RuleFor(c => c.RoleId).NotEmptyGuid();
    }
}
