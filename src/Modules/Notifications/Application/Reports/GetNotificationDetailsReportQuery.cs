using Application.Abstractions.Messaging;
using Application.Abstractions.Notifications;
using Domain.Notifications;

namespace Application.Notifications;

public sealed record GetNotificationDetailsReportQuery(
    int? Page,
    int? PageIndex,
    int? PageSize,
    DateTime? From,
    DateTime? To,
    NotificationChannel? Channel,
    NotificationStatus? Status) : IQuery<IResult>;

internal sealed class GetNotificationDetailsReportQueryHandler(INotificationUseCaseService service) : ResultWrappingQueryHandler<GetNotificationDetailsReportQuery>
{
    protected override async Task<IResult> HandleCore(GetNotificationDetailsReportQuery query, CancellationToken cancellationToken) =>
        await service.ReportDetailsAsync(
            query.Page,
            query.PageIndex,
            query.PageSize,
            query.From,
            query.To,
            query.Channel,
            query.Status,
            cancellationToken);
}





