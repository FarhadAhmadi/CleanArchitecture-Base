using Application.Abstractions.Messaging;
using Application.Abstractions.Notifications;
using Domain.Notifications;

namespace Application.Notifications;

public sealed record GetNotificationDetailsReportQuery(NotificationReportDetailsRequest Request) : IQuery<IResult>;
internal sealed class GetNotificationDetailsReportQueryHandler(INotificationUseCaseService service) : ResultWrappingQueryHandler<GetNotificationDetailsReportQuery>
{
    protected override async Task<IResult> HandleCore(GetNotificationDetailsReportQuery query, CancellationToken cancellationToken) =>
        await service.ReportDetailsAsync(query.Request, cancellationToken);
}





