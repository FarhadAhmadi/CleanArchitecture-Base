using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Domain.Profiles;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;

namespace Application.Profiles;

public sealed record GetProfilesAdminReportQuery(
    int? Page,
    int? PageIndex,
    int? PageSize,
    string? Search,
    bool? IsProfilePublic,
    string? PreferredLanguage,
    int? MinCompleteness,
    int? MaxCompleteness,
    DateTime? UpdatedFrom,
    DateTime? UpdatedTo) : IQuery<IResult>;

internal sealed class GetProfilesAdminReportQueryValidator : AbstractValidator<GetProfilesAdminReportQuery>
{
    public GetProfilesAdminReportQueryValidator()
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

internal sealed class GetProfilesAdminReportQueryHandler(
    IApplicationReadDbContext readContext,
    IValidator<GetProfilesAdminReportQuery> validator) : ResultWrappingQueryHandler<GetProfilesAdminReportQuery>
{
    protected override async Task<IResult> HandleCore(GetProfilesAdminReportQuery query, CancellationToken cancellationToken) =>
        await GetAsync(query, readContext, validator, cancellationToken);

    private static async Task<IResult> GetAsync(
        GetProfilesAdminReportQuery request,
        IApplicationReadDbContext readContext,
        IValidator<GetProfilesAdminReportQuery> validator,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return validationResult.ToValidationProblem();
        }

        IQueryable<UserProfile> query = readContext.UserProfiles;

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            string search = request.Search.Trim();
            query = query.Where(x =>
                x.DisplayName.Contains(search) ||
                x.Bio != null && x.Bio.Contains(search) ||
                x.Location != null && x.Location.Contains(search) ||
                x.ContactEmail != null && x.ContactEmail.Contains(search));
        }

        if (request.IsProfilePublic.HasValue)
        {
            bool isPublic = request.IsProfilePublic.Value;
            query = query.Where(x => x.IsProfilePublic == isPublic);
        }

        if (!string.IsNullOrWhiteSpace(request.PreferredLanguage))
        {
            string language = request.PreferredLanguage.Trim();
            query = query.Where(x => x.PreferredLanguage == language);
        }

        if (request.MinCompleteness.HasValue)
        {
            int min = request.MinCompleteness.Value;
            query = query.Where(x => x.ProfileCompletenessScore >= min);
        }

        if (request.MaxCompleteness.HasValue)
        {
            int max = request.MaxCompleteness.Value;
            query = query.Where(x => x.ProfileCompletenessScore <= max);
        }

        if (request.UpdatedFrom.HasValue)
        {
            DateTime from = request.UpdatedFrom.Value;
            query = query.Where(x => x.LastProfileUpdateAtUtc.HasValue && x.LastProfileUpdateAtUtc >= from);
        }

        if (request.UpdatedTo.HasValue)
        {
            DateTime to = request.UpdatedTo.Value;
            query = query.Where(x => x.LastProfileUpdateAtUtc.HasValue && x.LastProfileUpdateAtUtc <= to);
        }

        int page = request.PageIndex ?? request.Page ?? 1;
        int pageSize = request.PageSize ?? 50;
        (page, pageSize) = Application.Abstractions.Data.QueryableExtensions.NormalizePaging(page, pageSize, 50, 200);
        int total = await query.CountAsync(cancellationToken);

        List<object> items = await (
            from profile in query
            join user in readContext.Users on profile.UserId equals user.Id
            orderby profile.LastProfileUpdateAtUtc descending, profile.AuditCreatedAtUtc descending
            select new
            {
                profile.Id,
                profile.UserId,
                profile.DisplayName,
                profile.PreferredLanguage,
                profile.IsProfilePublic,
                profile.ProfileCompletenessScore,
                profile.LastProfileUpdateAtUtc,
                profile.LastSeenAtUtc,
                profile.ReceiveSecurityAlerts,
                profile.ReceiveProductUpdates,
                HasAvatar = profile.AvatarFileId.HasValue || profile.AvatarUrl != null,
                HasMusic = profile.FavoriteMusicFileId.HasValue ||
                           profile.FavoriteMusicTitle != null ||
                           profile.FavoriteMusicArtist != null,
                UserEmail = user.Email,
                UserPhone = user.PhoneNumber,
                user.EmailConfirmed
            })
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Cast<object>()
            .ToListAsync(cancellationToken);

        return Results.Ok(new
        {
            page,
            pageSize,
            total,
            items
        });
    }
}






