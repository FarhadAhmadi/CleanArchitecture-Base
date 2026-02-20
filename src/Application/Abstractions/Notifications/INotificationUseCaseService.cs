using Domain.Notifications;

namespace Application.Abstractions.Notifications;

public interface INotificationUseCaseService
{
    Task<IResult> CreateNotificationAsync(
        NotificationChannel channel,
        NotificationPriority priority,
        string recipient,
        string? subject,
        string? body,
        string language,
        Guid? templateId,
        DateTime? scheduledAtUtc,
        CancellationToken cancellationToken);
    Task<IResult> GetNotificationAsync(Guid notificationId, CancellationToken cancellationToken);
    Task<IResult> ListNotificationsAsync(
        int? page,
        int? pageIndex,
        int? pageSize,
        NotificationChannel? channel,
        NotificationStatus? status,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken);
    Task<IResult> CreateTemplateAsync(
        string name,
        NotificationChannel channel,
        string language,
        string subjectTemplate,
        string bodyTemplate,
        CancellationToken cancellationToken);
    Task<IResult> GetTemplateAsync(Guid templateId, CancellationToken cancellationToken);
    Task<IResult> UpdateTemplateAsync(Guid templateId, string subjectTemplate, string bodyTemplate, CancellationToken cancellationToken);
    Task<IResult> DeleteTemplateAsync(Guid templateId, CancellationToken cancellationToken);
    Task<IResult> ListTemplatesAsync(string? language, NotificationChannel? channel, CancellationToken cancellationToken);
    Task<IResult> ScheduleNotificationAsync(Guid notificationId, DateTime runAtUtc, string? ruleName, CancellationToken cancellationToken);
    Task<IResult> ListSchedulesAsync(CancellationToken cancellationToken);
    Task<IResult> DeleteScheduleAsync(Guid scheduleId, CancellationToken cancellationToken);
    Task<IResult> UpsertPermissionAsync(Guid notificationId, string subjectType, string subjectValue, bool canRead, bool canManage, CancellationToken cancellationToken);
    Task<IResult> GetPermissionsAsync(Guid notificationId, CancellationToken cancellationToken);
    Task<IResult> ArchiveAsync(Guid id, CancellationToken cancellationToken);
    Task<IResult> ReportSummaryAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken);
    Task<IResult> ReportDetailsAsync(
        int? page,
        int? pageIndex,
        int? pageSize,
        DateTime? from,
        DateTime? to,
        NotificationChannel? channel,
        NotificationStatus? status,
        CancellationToken cancellationToken);
}
