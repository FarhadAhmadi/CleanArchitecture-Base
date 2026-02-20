namespace Web.Api.Endpoints.Profiles;

public sealed record UpdateProfileSocialLinksRequest(Dictionary<string, string>? Links);
