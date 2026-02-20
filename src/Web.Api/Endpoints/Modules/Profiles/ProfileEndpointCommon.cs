using System.Text.Json;
using Application.Abstractions.Data;
using Domain.Profiles;
using Microsoft.EntityFrameworkCore;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Profiles;

internal static class ProfileEndpointCommon
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    internal static async Task<UserProfile?> GetCurrentProfileForUpdateAsync(
        Guid userId,
        IApplicationDbContext writeContext,
        CancellationToken cancellationToken)
    {
        return await writeContext.UserProfiles
            .SingleOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }

    internal static object ToPrivateResponse(UserProfile profile)
    {
        return new
        {
            profile.Id,
            profile.UserId,
            profile.DisplayName,
            profile.Bio,
            profile.AvatarUrl,
            profile.AvatarFileId,
            profile.DateOfBirth,
            profile.Gender,
            profile.Location,
            profile.TimeZone,
            profile.PreferredLanguage,
            profile.WebsiteUrl,
            profile.ContactEmail,
            profile.ContactPhone,
            profile.FavoriteMusicTitle,
            profile.FavoriteMusicArtist,
            profile.FavoriteMusicFileId,
            interests = ParseInterests(profile.InterestsCsv),
            socialLinks = ParseSocialLinks(profile.SocialLinksJson),
            profile.IsProfilePublic,
            profile.ShowEmail,
            profile.ShowPhone,
            profile.ReceiveSecurityAlerts,
            profile.ReceiveProductUpdates,
            profile.ProfileCompletenessScore,
            profile.LastSeenAtUtc,
            profile.LastProfileUpdateAtUtc,
            profile.AuditCreatedAtUtc,
            profile.AuditUpdatedAtUtc
        };
    }

    internal static object ToPublicResponse(UserProfile profile)
    {
        return new
        {
            profile.UserId,
            profile.DisplayName,
            profile.Bio,
            profile.AvatarUrl,
            profile.AvatarFileId,
            profile.Location,
            profile.WebsiteUrl,
            profile.PreferredLanguage,
            profile.FavoriteMusicTitle,
            profile.FavoriteMusicArtist,
            profile.FavoriteMusicFileId,
            interests = ParseInterests(profile.InterestsCsv),
            socialLinks = ParseSocialLinks(profile.SocialLinksJson),
            profile.ProfileCompletenessScore,
            profile.LastProfileUpdateAtUtc,
            ContactEmail = profile.ShowEmail ? profile.ContactEmail : null,
            ContactPhone = profile.ShowPhone ? profile.ContactPhone : null
        };
    }

    internal static List<string> ParseInterests(string? csv)
    {
        return [.. (csv ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)];
    }

    internal static Dictionary<string, string> ParseSocialLinks(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json, JsonOptions) ?? [];
        }
        catch
        {
            return [];
        }
    }

    internal static string BuildSocialLinksJson(Dictionary<string, string>? links)
    {
        Dictionary<string, string> sanitized = new(StringComparer.OrdinalIgnoreCase);
        foreach ((string key, string value) in links ?? [])
        {
            string normalizedKey = InputSanitizer.SanitizeIdentifier(key, 50) ?? string.Empty;
            string normalizedValue = InputSanitizer.SanitizeText(value, 800) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(normalizedKey) || string.IsNullOrWhiteSpace(normalizedValue))
            {
                continue;
            }

            sanitized[normalizedKey] = normalizedValue;
        }

        return JsonSerializer.Serialize(sanitized, JsonOptions);
    }

    internal static int ComputeCompleteness(UserProfile profile)
    {
        int total = 11;
        int score = 0;

        if (!string.IsNullOrWhiteSpace(profile.DisplayName))
        {
            score++;
        }

        if (!string.IsNullOrWhiteSpace(profile.Bio))
        {
            score++;
        }

        if (!string.IsNullOrWhiteSpace(profile.AvatarUrl) || profile.AvatarFileId.HasValue)
        {
            score++;
        }

        if (!string.IsNullOrWhiteSpace(profile.Location))
        {
            score++;
        }

        if (!string.IsNullOrWhiteSpace(profile.TimeZone))
        {
            score++;
        }

        if (!string.IsNullOrWhiteSpace(profile.WebsiteUrl))
        {
            score++;
        }

        if (!string.IsNullOrWhiteSpace(profile.ContactEmail))
        {
            score++;
        }

        if (!string.IsNullOrWhiteSpace(profile.ContactPhone))
        {
            score++;
        }

        if (ParseInterests(profile.InterestsCsv).Count > 0)
        {
            score++;
        }

        if (ParseSocialLinks(profile.SocialLinksJson).Count > 0)
        {
            score++;
        }

        if (!string.IsNullOrWhiteSpace(profile.FavoriteMusicTitle) ||
            !string.IsNullOrWhiteSpace(profile.FavoriteMusicArtist) ||
            profile.FavoriteMusicFileId.HasValue)
        {
            score++;
        }

        return (int)Math.Round(score / (double)total * 100, MidpointRounding.AwayFromZero);
    }
}
