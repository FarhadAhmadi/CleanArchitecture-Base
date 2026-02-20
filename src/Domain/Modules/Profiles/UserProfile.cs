using SharedKernel;

namespace Domain.Profiles;

public sealed class UserProfile : Entity
{
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public Guid? AvatarFileId { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? Location { get; set; }
    public string? TimeZone { get; set; }
    public string PreferredLanguage { get; set; } = "fa-IR";
    public string? WebsiteUrl { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? FavoriteMusicTitle { get; set; }
    public string? FavoriteMusicArtist { get; set; }
    public Guid? FavoriteMusicFileId { get; set; }
    public string InterestsCsv { get; set; } = string.Empty;
    public string SocialLinksJson { get; set; } = "{}";
    public bool IsProfilePublic { get; set; } = true;
    public bool ShowEmail { get; set; }
    public bool ShowPhone { get; set; }
    public bool ReceiveSecurityAlerts { get; set; } = true;
    public bool ReceiveProductUpdates { get; set; } = true;
    public int ProfileCompletenessScore { get; set; }
    public DateTime? LastSeenAtUtc { get; set; }
    public DateTime? LastProfileUpdateAtUtc { get; set; }
}
