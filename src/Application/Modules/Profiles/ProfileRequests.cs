using FluentValidation;

namespace Application.Profiles;

public sealed record CreateProfileRequest(string? DisplayName, string? PreferredLanguage, bool IsProfilePublic);
public sealed record AddProfileInterestsRequest(List<string>? Interests);
public sealed record RemoveProfileInterestRequest(string Interest);
public sealed record UpdateProfileAvatarRequest(Guid? AvatarFileId);
public sealed record UpdateProfileBasicRequest(string? DisplayName, string? Bio, DateTime? DateOfBirth, string? Gender, string? Location);
public sealed record UpdateProfileBioRequest(string? Bio);
public sealed record UpdateProfileContactRequest(string? ContactEmail, string? ContactPhone, string? Website, string? TimeZone);
public sealed record UpdateProfileMusicRequest(string? MusicTitle, string? MusicArtist, Guid? MusicFileId);
public sealed record UpdateProfilePreferencesRequest(string? PreferredLanguage, bool ReceiveSecurityAlerts, bool ReceiveProductUpdates);
public sealed record UpdateProfilePrivacyRequest(bool IsProfilePublic, bool ShowEmail, bool ShowPhone);
public sealed record UpdateProfileSocialLinksRequest(Dictionary<string, string>? Links);

public sealed class ProfileAdminReportRequest
{
    public int? Page { get; set; }
    public int? PageIndex { get; set; }
    public int? PageSize { get; set; }
    public string? Search { get; set; }
    public bool? IsProfilePublic { get; set; }
    public string? PreferredLanguage { get; set; }
    public int? MinCompleteness { get; set; }
    public int? MaxCompleteness { get; set; }
    public DateTime? UpdatedFrom { get; set; }
    public DateTime? UpdatedTo { get; set; }

    public (int Page, int PageSize) NormalizePaging()
    {
        int page = PageIndex ?? Page ?? 1;
        int pageSize = PageSize ?? 50;
        return Application.Abstractions.Data.QueryableExtensions.NormalizePaging(page, pageSize, 50, 200);
    }
}

internal sealed class CreateProfileRequestValidator : AbstractValidator<CreateProfileRequest>
{
    public CreateProfileRequestValidator()
    {
        RuleFor(x => x.DisplayName).NotEmpty().MaximumLength(160);
        RuleFor(x => x.PreferredLanguage).MaximumLength(16).When(x => !string.IsNullOrWhiteSpace(x.PreferredLanguage));
    }
}

internal sealed class AddProfileInterestsRequestValidator : AbstractValidator<AddProfileInterestsRequest>
{
    public AddProfileInterestsRequestValidator()
    {
        RuleFor(x => x.Interests).NotNull().Must(x => x!.Count is > 0 and <= 20);
        RuleForEach(x => x.Interests!).NotEmpty().MaximumLength(60);
    }
}

internal sealed class RemoveProfileInterestRequestValidator : AbstractValidator<RemoveProfileInterestRequest>
{
    public RemoveProfileInterestRequestValidator() => RuleFor(x => x.Interest).NotEmpty().MaximumLength(60);
}

internal sealed class UpdateProfileAvatarRequestValidator : AbstractValidator<UpdateProfileAvatarRequest>
{
    public UpdateProfileAvatarRequestValidator() { }
}

internal sealed class UpdateProfileBasicRequestValidator : AbstractValidator<UpdateProfileBasicRequest>
{
    public UpdateProfileBasicRequestValidator()
    {
        RuleFor(x => x.DisplayName).MaximumLength(160).When(x => !string.IsNullOrWhiteSpace(x.DisplayName));
        RuleFor(x => x.Bio).MaximumLength(1200).When(x => !string.IsNullOrWhiteSpace(x.Bio));
        RuleFor(x => x.Gender).MaximumLength(32).When(x => !string.IsNullOrWhiteSpace(x.Gender));
        RuleFor(x => x.Location).MaximumLength(200).When(x => !string.IsNullOrWhiteSpace(x.Location));
        RuleFor(x => x.DateOfBirth).LessThanOrEqualTo(DateTime.UtcNow.Date).When(x => x.DateOfBirth.HasValue);
    }
}

internal sealed class UpdateProfileBioRequestValidator : AbstractValidator<UpdateProfileBioRequest>
{
    public UpdateProfileBioRequestValidator() => RuleFor(x => x.Bio).MaximumLength(1200).When(x => !string.IsNullOrWhiteSpace(x.Bio));
}

internal sealed class UpdateProfileContactRequestValidator : AbstractValidator<UpdateProfileContactRequest>
{
    public UpdateProfileContactRequestValidator()
    {
        RuleFor(x => x.ContactEmail).MaximumLength(320).When(x => !string.IsNullOrWhiteSpace(x.ContactEmail));
        RuleFor(x => x.ContactPhone).MaximumLength(32).When(x => !string.IsNullOrWhiteSpace(x.ContactPhone));
        RuleFor(x => x.Website).MaximumLength(400).When(x => !string.IsNullOrWhiteSpace(x.Website));
        RuleFor(x => x.TimeZone).MaximumLength(80).When(x => !string.IsNullOrWhiteSpace(x.TimeZone));
    }
}

internal sealed class UpdateProfileMusicRequestValidator : AbstractValidator<UpdateProfileMusicRequest>
{
    public UpdateProfileMusicRequestValidator()
    {
        RuleFor(x => x.MusicTitle).MaximumLength(200).When(x => !string.IsNullOrWhiteSpace(x.MusicTitle));
        RuleFor(x => x.MusicArtist).MaximumLength(200).When(x => !string.IsNullOrWhiteSpace(x.MusicArtist));
    }
}

internal sealed class UpdateProfilePreferencesRequestValidator : AbstractValidator<UpdateProfilePreferencesRequest>
{
    public UpdateProfilePreferencesRequestValidator()
    {
        RuleFor(x => x.PreferredLanguage).MaximumLength(16).When(x => !string.IsNullOrWhiteSpace(x.PreferredLanguage));
    }
}

internal sealed class UpdateProfilePrivacyRequestValidator : AbstractValidator<UpdateProfilePrivacyRequest>
{
    public UpdateProfilePrivacyRequestValidator() { }
}

internal sealed class UpdateProfileSocialLinksRequestValidator : AbstractValidator<UpdateProfileSocialLinksRequest>
{
    public UpdateProfileSocialLinksRequestValidator()
    {
        RuleFor(x => x.Links).Must(x => x is null || x.Count <= 20);
        RuleForEach(x => x.Links!).ChildRules(link =>
        {
            link.RuleFor(x => x.Key).NotEmpty().MaximumLength(50);
            link.RuleFor(x => x.Value).NotEmpty().MaximumLength(800);
        });
    }
}

internal sealed class ProfileAdminReportRequestValidator : AbstractValidator<ProfileAdminReportRequest>
{
    public ProfileAdminReportRequestValidator()
    {
        RuleFor(x => x.Search).MaximumLength(100).When(x => !string.IsNullOrWhiteSpace(x.Search));
        RuleFor(x => x.PreferredLanguage).MaximumLength(16).When(x => !string.IsNullOrWhiteSpace(x.PreferredLanguage));
        RuleFor(x => x.MinCompleteness).InclusiveBetween(0, 100).When(x => x.MinCompleteness.HasValue);
        RuleFor(x => x.MaxCompleteness).InclusiveBetween(0, 100).When(x => x.MaxCompleteness.HasValue);
        RuleFor(x => x).Must(x => !x.MinCompleteness.HasValue || !x.MaxCompleteness.HasValue || x.MinCompleteness <= x.MaxCompleteness)
            .WithMessage("minCompletion cannot be greater than maxCompletion.");
        RuleFor(x => x).Must(x => !x.UpdatedFrom.HasValue || !x.UpdatedTo.HasValue || x.UpdatedFrom <= x.UpdatedTo)
            .WithMessage("updatedFrom cannot be greater than updatedTo.");
    }
}




