using FluentValidation;

namespace Application.Authorization.AssignPermissionToRole;

internal sealed class AssignPermissionToRoleCommandValidator : AbstractValidator<AssignPermissionToRoleCommand>
{
    public AssignPermissionToRoleCommandValidator()
    {
        RuleFor(c => c.RoleName).NotEmpty().MaximumLength(100);
        RuleFor(c => c.PermissionCode).NotEmpty().MaximumLength(200);
    }
}
