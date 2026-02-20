using Domain.Notifications;
using Application.Abstractions.Data;

namespace Application.Notifications;

public sealed record CreateNotificationRequest(
    NotificationChannel Channel,
    NotificationPriority Priority,
    string Recipient,
    string? Subject,
    string? Body,
    string Language,
    Guid? TemplateId,
    DateTime? ScheduledAtUtc);

public sealed record CreateNotificationTemplateRequest(
    string Name,
    NotificationChannel Channel,
    string Language,
    string SubjectTemplate,
    string BodyTemplate);

public sealed record UpdateNotificationTemplateRequest(
    string SubjectTemplate,
    string BodyTemplate);

public sealed record ScheduleNotificationRequest(DateTime RunAtUtc, string? RuleName);

public sealed record UpsertNotificationPermissionRequest(string SubjectType, string SubjectValue, bool CanRead, bool CanManage);

public sealed class ListNotificationsRequest
{
    public int? Page { get; set; }
    public int? PageIndex { get; set; }
    public int? PageSize { get; set; }
    public NotificationChannel? Channel { get; set; }
    public NotificationStatus? Status { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }

    public (int Page, int PageSize) NormalizePaging()
    {
        int page = PageIndex ?? Page ?? 1;
        int pageSize = PageSize ?? 50;
        return QueryableExtensions.NormalizePaging(page, pageSize, 50, 200);
    }
}

public sealed class NotificationReportDetailsRequest
{
    public int? Page { get; set; }
    public int? PageIndex { get; set; }
    public int? PageSize { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public NotificationChannel? Channel { get; set; }
    public NotificationStatus? Status { get; set; }

    public (int Page, int PageSize) NormalizePaging()
    {
        int page = PageIndex ?? Page ?? 1;
        int pageSize = PageSize ?? 50;
        return QueryableExtensions.NormalizePaging(page, pageSize, 50, 200);
    }
}




