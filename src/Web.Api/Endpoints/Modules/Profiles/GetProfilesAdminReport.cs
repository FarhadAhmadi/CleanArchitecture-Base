using Application.Abstractions.Data;
using Domain.Profiles;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Profiles;

internal sealed class GetProfilesAdminReport : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("profiles/reports/admin", GetAsync)
            .HasPermission(Permissions.ProfilesAdminRead)
            .WithTags(Tags.Profiles);
    }

    private static async Task<IResult> GetAsync(
        [AsParameters] ProfileAdminReportRequest request,
        IApplicationReadDbContext readContext,
        IValidator<ProfileAdminReportRequest> validator,
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

        (int page, int pageSize) = request.NormalizePaging();
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
