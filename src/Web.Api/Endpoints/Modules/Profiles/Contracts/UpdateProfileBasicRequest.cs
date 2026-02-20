namespace Web.Api.Endpoints.Profiles;

public sealed record UpdateProfileBasicRequest(
    string? DisplayName,
    string? Bio,
    DateTime? DateOfBirth,
    string? Gender,
    string? Location);
