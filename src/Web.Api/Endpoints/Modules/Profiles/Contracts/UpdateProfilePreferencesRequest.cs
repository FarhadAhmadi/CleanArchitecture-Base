namespace Web.Api.Endpoints.Profiles;

public sealed record UpdateProfilePreferencesRequest(
    string? PreferredLanguage,
    bool ReceiveSecurityAlerts,
    bool ReceiveProductUpdates);
