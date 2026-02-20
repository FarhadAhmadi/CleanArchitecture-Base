namespace Web.Api.Endpoints.Profiles;

public sealed record UpdateProfilePrivacyRequest(
    bool IsProfilePublic,
    bool ShowEmail,
    bool ShowPhone);
