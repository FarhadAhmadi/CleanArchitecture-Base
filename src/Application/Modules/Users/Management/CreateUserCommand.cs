using Application.Abstractions.Messaging;

namespace Application.Users.Management;

public sealed record CreateUserCommand(
    string Email,
    string FirstName,
    string LastName,
    string Password,
    string? PhoneNumber,
    bool? EmailConfirmed,
    bool? PhoneNumberConfirmed,
    bool? TwoFactorEnabled,
    bool? LockoutEnabled,
    DateTime? LockoutEndUtc,
    int? FailedLoginCount) : ICommand<UserAdminResponse>;
