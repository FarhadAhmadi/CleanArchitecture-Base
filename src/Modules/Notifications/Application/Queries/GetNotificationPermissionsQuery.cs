using Application.Abstractions.Messaging;
using Application.Abstractions.Notifications;
using Domain.Notifications;

namespace Application.Notifications;

public sealed record GetNotificationPermissionsQuery(Guid NotificationId) : IQuery<IResult>;
internal sealed class GetNotificationPermissionsQueryHandler(INotificationUseCaseService service) : ResultWrappingQueryHandler<GetNotificationPermissionsQuery>
{
    protected override async Task<IResult> HandleCore(GetNotificationPermissionsQuery query, CancellationToken cancellationToken) =>
        await service.GetPermissionsAsync(query.NotificationId, cancellationToken);
}





