using Application.Abstractions.Validation;
using FluentValidation;

namespace Application.Users.Management;

internal sealed class DeleteUserCommandValidator : AbstractValidator<DeleteUserCommand>
{
    public DeleteUserCommandValidator()
    {
        RuleFor(c => c.UserId).NotEmptyGuid();
    }
}
