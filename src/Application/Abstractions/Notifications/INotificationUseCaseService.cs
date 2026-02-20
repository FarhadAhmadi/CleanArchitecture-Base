using Application.Notifications;

namespace Application.Abstractions.Notifications;

public interface INotificationUseCaseService
{
    Task<IResult> CreateNotificationAsync(CreateNotificationRequest request, CancellationToken cancellationToken);
    Task<IResult> GetNotificationAsync(Guid notificationId, CancellationToken cancellationToken);
    Task<IResult> ListNotificationsAsync(ListNotificationsRequest request, CancellationToken cancellationToken);
    Task<IResult> CreateTemplateAsync(CreateNotificationTemplateRequest request, CancellationToken cancellationToken);
    Task<IResult> GetTemplateAsync(Guid templateId, CancellationToken cancellationToken);
    Task<IResult> UpdateTemplateAsync(Guid templateId, UpdateNotificationTemplateRequest request, CancellationToken cancellationToken);
    Task<IResult> DeleteTemplateAsync(Guid templateId, CancellationToken cancellationToken);
    Task<IResult> ListTemplatesAsync(string? language, Domain.Notifications.NotificationChannel? channel, CancellationToken cancellationToken);
    Task<IResult> ScheduleNotificationAsync(Guid notificationId, ScheduleNotificationRequest request, CancellationToken cancellationToken);
    Task<IResult> ListSchedulesAsync(CancellationToken cancellationToken);
    Task<IResult> DeleteScheduleAsync(Guid scheduleId, CancellationToken cancellationToken);
    Task<IResult> UpsertPermissionAsync(Guid notificationId, UpsertNotificationPermissionRequest request, CancellationToken cancellationToken);
    Task<IResult> GetPermissionsAsync(Guid notificationId, CancellationToken cancellationToken);
    Task<IResult> ArchiveAsync(Guid id, CancellationToken cancellationToken);
    Task<IResult> ReportSummaryAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken);
    Task<IResult> ReportDetailsAsync(NotificationReportDetailsRequest request, CancellationToken cancellationToken);
}
