using Application.Abstractions.Users;
using Domain.Users;
using SharedKernel;

namespace Application.Users.Register;

internal sealed class UserRegisteredDomainEventHandler(
    IUserRegistrationVerificationService verificationService)
    : IDomainEventHandler<UserRegisteredDomainEvent>
{
    public async Task Handle(UserRegisteredDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        await verificationService.QueueVerificationAsync(domainEvent.UserId, cancellationToken);
    }
}
