namespace Web.Api.Endpoints.Profiles;

public sealed record CreateProfileRequest(string? DisplayName, string? PreferredLanguage, bool IsProfilePublic);
