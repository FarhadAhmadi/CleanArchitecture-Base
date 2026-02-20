using FluentValidation;

namespace Web.Api.Endpoints.Profiles;

internal sealed class UpdateProfileAvatarRequestValidator : AbstractValidator<UpdateProfileAvatarRequest>
{
    public UpdateProfileAvatarRequestValidator()
    {
        RuleFor(x => x.AvatarFileId)
            .NotNull()
            .NotEqual(Guid.Empty);
    }
}
