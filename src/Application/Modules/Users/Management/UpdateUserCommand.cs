using Application.Abstractions.Messaging;

namespace Application.Users.Management;

public sealed record UpdateUserCommand(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName) : ICommand<UserAdminResponse>;
