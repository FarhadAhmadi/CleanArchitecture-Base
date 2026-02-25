using Application.Abstractions.Messaging;

namespace Application.Users.Management;

public sealed record UpdateUserCommand(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    bool? EmailConfirmed,
    bool? PhoneNumberConfirmed,
    bool? TwoFactorEnabled,
    bool? LockoutEnabled,
    DateTime? LockoutEndUtc,
    bool? ClearLockoutEnd,
    int? FailedLoginCount) : ICommand<UserAdminResponse>;
