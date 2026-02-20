namespace Web.Api.Endpoints.Profiles;

public sealed record UpdateProfileMusicRequest(
    string? MusicTitle,
    string? MusicArtist,
    Guid? MusicFileId);
