using FluentValidation;

namespace Web.Api.Endpoints.Profiles;

internal sealed class UpdateProfileMusicRequestValidator : AbstractValidator<UpdateProfileMusicRequest>
{
    public UpdateProfileMusicRequestValidator()
    {
        RuleFor(x => x.MusicTitle)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.MusicTitle));

        RuleFor(x => x.MusicArtist)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.MusicArtist));

        RuleFor(x => x.MusicFileId)
            .NotNull()
            .NotEqual(Guid.Empty);
    }
}
