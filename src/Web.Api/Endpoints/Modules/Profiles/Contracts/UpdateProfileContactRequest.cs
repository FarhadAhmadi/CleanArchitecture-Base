namespace Web.Api.Endpoints.Profiles;

public sealed record UpdateProfileContactRequest(
    string? ContactEmail,
    string? ContactPhone,
    string? Website,
    string? TimeZone);
