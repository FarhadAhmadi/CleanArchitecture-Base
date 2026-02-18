using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Domain.Notifications;
using Infrastructure.Notifications;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Api.Endpoints.Users;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Notifications;

internal sealed class Manage : IEndpoint
{
    private const string SubjectTypeUser = "User";
    private const string SubjectTypeRole = "Role";

    public sealed record CreateNotificationRequest(
        NotificationChannel Channel,
        NotificationPriority Priority,
        string Recipient,
        string? Subject,
        string? Body,
        Guid? TemplateId,
        string Language,
        DateTime? ScheduledAtUtc);

    public sealed record CreateTemplateRequest(
        string Name,
        NotificationChannel Channel,
        string Language,
        string SubjectTemplate,
        string BodyTemplate);

    public sealed record UpdateTemplateRequest(
        string SubjectTemplate,
        string BodyTemplate);

    public sealed record ScheduleRequest(DateTime RunAtUtc, string? RuleName);
    public sealed record PermissionRequest(string SubjectType, string SubjectValue, bool CanRead, bool CanManage);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        RouteGroupBuilder notifications = app.MapGroup("notifications").WithTags(Tags.Notifications);
        RouteGroupBuilder templates = app.MapGroup("notification-templates").WithTags(Tags.Notifications);

        notifications.MapPost("/", CreateNotificationAsync).HasPermission(Permissions.NotificationsWrite);
        notifications.MapGet("/{notificationId:guid}", GetNotificationAsync).HasPermission(Permissions.NotificationsRead);
        notifications.MapGet("/", ListNotificationsAsync).HasPermission(Permissions.NotificationsRead);

        templates.MapPost("/", CreateTemplateAsync).HasPermission(Permissions.NotificationTemplatesManage);
        templates.MapGet("/{templateId:guid}", GetTemplateAsync).HasPermission(Permissions.NotificationTemplatesManage);
        templates.MapPut("/{templateId:guid}", UpdateTemplateAsync).HasPermission(Permissions.NotificationTemplatesManage);
        templates.MapDelete("/{templateId:guid}", DeleteTemplateAsync).HasPermission(Permissions.NotificationTemplatesManage);
        templates.MapGet("/", ListTemplatesAsync).HasPermission(Permissions.NotificationTemplatesManage);

        notifications.MapPost("/{notificationId:guid}/schedule", ScheduleNotificationAsync).HasPermission(Permissions.NotificationSchedulesManage);
        notifications.MapGet("/schedules", ListSchedulesAsync).HasPermission(Permissions.NotificationSchedulesManage);
        notifications.MapDelete("/schedules/{scheduleId:guid}", DeleteScheduleAsync).HasPermission(Permissions.NotificationSchedulesManage);

        notifications.MapPost("/{notificationId:guid}/permissions", UpsertPermissionAsync).HasPermission(Permissions.NotificationPermissionsManage);
        notifications.MapGet("/{notificationId:guid}/permissions", GetPermissionsAsync).HasPermission(Permissions.NotificationPermissionsManage);

        notifications.MapDelete("/archive/{id:guid}", ArchiveAsync).HasPermission(Permissions.NotificationsWrite);
        notifications.MapGet("/reports/summary", ReportSummaryAsync).HasPermission(Permissions.NotificationReportsRead);
        notifications.MapGet("/reports/details", ReportDetailsAsync).HasPermission(Permissions.NotificationReportsRead);
    }

    private static async Task<IResult> CreateNotificationAsync(
        CreateNotificationRequest request,
        IUserContext userContext,
        IApplicationDbContext writeContext,
        NotificationSensitiveDataProtector protector,
        NotificationOptions options,
        CancellationToken cancellationToken)
    {
        if (!options.Enabled)
        {
            return Results.BadRequest(new { message = "Notification module is disabled." });
        }

        DateTime now = DateTime.UtcNow;
        DateTime from = now.AddMinutes(-1);
        int sentInLastMinute = await writeContext.NotificationMessages
            .CountAsync(x => x.CreatedByUserId == userContext.UserId && x.CreatedAtUtc >= from, cancellationToken);

        if (sentInLastMinute >= Math.Max(1, options.PerUserPerMinuteLimit))
        {
            return Results.StatusCode(StatusCodes.Status429TooManyRequests);
        }

        string recipient = (request.Recipient ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(recipient))
        {
            return Results.BadRequest(new { message = "Recipient is required." });
        }

        (string subject, string body) = await ResolveContentAsync(writeContext, request, cancellationToken);

        NotificationMessage message = new()
        {
            Id = Guid.NewGuid(),
            CreatedByUserId = userContext.UserId,
            Channel = request.Channel,
            Priority = request.Priority,
            Status = request.ScheduledAtUtc.HasValue && request.ScheduledAtUtc.Value > now
                ? NotificationStatus.Scheduled
                : NotificationStatus.Pending,
            RecipientEncrypted = protector.Protect(recipient),
            RecipientHash = NotificationSensitiveDataProtector.ComputeDeterministicHash(recipient),
            Subject = subject,
            Body = body,
            Language = string.IsNullOrWhiteSpace(request.Language) ? "fa-IR" : request.Language.Trim(),
            TemplateId = request.TemplateId,
            CreatedAtUtc = now,
            ScheduledAtUtc = request.ScheduledAtUtc,
            MaxRetryCount = Math.Max(1, options.MaxRetries)
        };

        writeContext.NotificationMessages.Add(message);

        if (message.ScheduledAtUtc.HasValue && message.ScheduledAtUtc.Value > now)
        {
            writeContext.NotificationSchedules.Add(new NotificationSchedule
            {
                Id = Guid.NewGuid(),
                NotificationId = message.Id,
                RunAtUtc = message.ScheduledAtUtc.Value,
                CreatedAtUtc = now
            });
        }

        await writeContext.SaveChangesAsync(cancellationToken);
        return Results.Ok(new { notificationId = message.Id, message.Status, message.Channel, message.Priority });
    }

    private static async Task<IResult> GetNotificationAsync(
        Guid notificationId,
        IUserContext userContext,
        IApplicationReadDbContext readContext,
        CancellationToken cancellationToken)
    {
        NotificationMessage? notification = await readContext.NotificationMessages
            .SingleOrDefaultAsync(x => x.Id == notificationId, cancellationToken);

        if (notification is null)
        {
            return Results.NotFound();
        }

        if (!await CanReadAsync(notification, userContext.UserId, readContext, cancellationToken))
        {
            return Results.Forbid();
        }

        return Results.Ok(new
        {
            notification.Id,
            notification.Channel,
            notification.Priority,
            notification.Status,
            notification.Subject,
            notification.Language,
            notification.RetryCount,
            notification.MaxRetryCount,
            notification.CreatedAtUtc,
            notification.ScheduledAtUtc,
            notification.SentAtUtc,
            notification.DeliveredAtUtc,
            notification.LastError
        });
    }

    private static async Task<IResult> ListNotificationsAsync(
        NotificationChannel? channel,
        NotificationStatus? status,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize,
        IUserContext userContext,
        IApplicationReadDbContext readContext,
        CancellationToken cancellationToken)
    {
        IQueryable<NotificationMessage> query = readContext.NotificationMessages;
        bool isAdmin = await IsAdminAsync(userContext.UserId, readContext, cancellationToken);
        if (!isAdmin)
        {
            Guid uid = userContext.UserId;
            string uidText = uid.ToString("N");
            IQueryable<Guid> aclRead = readContext.NotificationPermissionEntries
                .Where(x => x.SubjectType == SubjectTypeUser && x.SubjectValue == uidText && x.CanRead)
                .Select(x => x.NotificationId);

            query = query.Where(x => x.CreatedByUserId == uid || aclRead.Contains(x.Id));
        }

        if (channel.HasValue)
        {
            query = query.Where(x => x.Channel == channel.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        if (from.HasValue)
        {
            query = query.Where(x => x.CreatedAtUtc >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(x => x.CreatedAtUtc <= to.Value);
        }

        (int normalizedPage, int normalizedPageSize) = Application.Abstractions.Data.QueryableExtensions.NormalizePaging(page, pageSize, 50, 200);
        int total = await query.CountAsync(cancellationToken);
        List<object> items = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .Select(x => new
            {
                x.Id,
                x.Channel,
                x.Priority,
                x.Status,
                x.Subject,
                x.CreatedAtUtc,
                x.ScheduledAtUtc
            })
            .Cast<object>()
            .ToListAsync(cancellationToken);

        return Results.Ok(new { page = normalizedPage, pageSize = normalizedPageSize, total, items });
    }

    private static async Task<IResult> CreateTemplateAsync(CreateTemplateRequest request, IUserContext userContext, IApplicationDbContext writeContext, CancellationToken cancellationToken)
    {
        NotificationTemplate template = new()
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Channel = request.Channel,
            Language = string.IsNullOrWhiteSpace(request.Language) ? "fa-IR" : request.Language.Trim(),
            SubjectTemplate = request.SubjectTemplate,
            BodyTemplate = request.BodyTemplate,
            CreatedAtUtc = DateTime.UtcNow
        };

        writeContext.NotificationTemplates.Add(template);
        writeContext.NotificationTemplateRevisions.Add(new NotificationTemplateRevision
        {
            Id = Guid.NewGuid(),
            TemplateId = template.Id,
            Version = template.Version,
            SubjectTemplate = template.SubjectTemplate,
            BodyTemplate = template.BodyTemplate,
            CreatedAtUtc = DateTime.UtcNow,
            ChangedByUserId = userContext.UserId
        });

        await writeContext.SaveChangesAsync(cancellationToken);
        return Results.Ok(new { templateId = template.Id, template.Version });
    }

    private static async Task<IResult> GetTemplateAsync(Guid templateId, IApplicationReadDbContext readContext, CancellationToken cancellationToken)
    {
        NotificationTemplate? template = await readContext.NotificationTemplates
            .SingleOrDefaultAsync(x => x.Id == templateId && !x.IsDeleted, cancellationToken);
        return template is null ? Results.NotFound() : Results.Ok(template);
    }

    private static async Task<IResult> UpdateTemplateAsync(Guid templateId, UpdateTemplateRequest request, IUserContext userContext, IApplicationDbContext writeContext, CancellationToken cancellationToken)
    {
        NotificationTemplate? template = await writeContext.NotificationTemplates
            .SingleOrDefaultAsync(x => x.Id == templateId && !x.IsDeleted, cancellationToken);
        if (template is null)
        {
            return Results.NotFound();
        }

        template.SubjectTemplate = request.SubjectTemplate;
        template.BodyTemplate = request.BodyTemplate;
        template.Version++;
        template.UpdatedAtUtc = DateTime.UtcNow;

        writeContext.NotificationTemplateRevisions.Add(new NotificationTemplateRevision
        {
            Id = Guid.NewGuid(),
            TemplateId = template.Id,
            Version = template.Version,
            SubjectTemplate = template.SubjectTemplate,
            BodyTemplate = template.BodyTemplate,
            CreatedAtUtc = DateTime.UtcNow,
            ChangedByUserId = userContext.UserId
        });

        await writeContext.SaveChangesAsync(cancellationToken);
        return Results.Ok(new { templateId = template.Id, template.Version });
    }

    private static async Task<IResult> DeleteTemplateAsync(Guid templateId, IApplicationDbContext writeContext, CancellationToken cancellationToken)
    {
        NotificationTemplate? template = await writeContext.NotificationTemplates
            .SingleOrDefaultAsync(x => x.Id == templateId && !x.IsDeleted, cancellationToken);
        if (template is null)
        {
            return Results.NotFound();
        }

        template.IsDeleted = true;
        template.UpdatedAtUtc = DateTime.UtcNow;
        await writeContext.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> ListTemplatesAsync(string? language, NotificationChannel? channel, IApplicationReadDbContext readContext, CancellationToken cancellationToken)
    {
        IQueryable<NotificationTemplate> query = readContext.NotificationTemplates.Where(x => !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(language))
        {
            query = query.Where(x => x.Language == language);
        }

        if (channel.HasValue)
        {
            query = query.Where(x => x.Channel == channel.Value);
        }

        List<NotificationTemplate> items = await query.OrderBy(x => x.Name).ToListAsync(cancellationToken);
        return Results.Ok(items);
    }

    private static async Task<IResult> ScheduleNotificationAsync(Guid notificationId, ScheduleRequest request, IApplicationDbContext writeContext, CancellationToken cancellationToken)
    {
        NotificationMessage? notification = await writeContext.NotificationMessages.SingleOrDefaultAsync(x => x.Id == notificationId, cancellationToken);
        if (notification is null)
        {
            return Results.NotFound();
        }

        notification.Status = NotificationStatus.Scheduled;
        notification.ScheduledAtUtc = request.RunAtUtc;
        notification.NextRetryAtUtc = request.RunAtUtc;

        NotificationSchedule? existing = await writeContext.NotificationSchedules
            .SingleOrDefaultAsync(x => x.NotificationId == notificationId && !x.IsCancelled, cancellationToken);

        if (existing is null)
        {
            writeContext.NotificationSchedules.Add(new NotificationSchedule
            {
                Id = Guid.NewGuid(),
                NotificationId = notificationId,
                RunAtUtc = request.RunAtUtc,
                RuleName = request.RuleName,
                CreatedAtUtc = DateTime.UtcNow
            });
        }
        else
        {
            existing.RunAtUtc = request.RunAtUtc;
            existing.RuleName = request.RuleName;
            existing.IsCancelled = false;
        }

        await writeContext.SaveChangesAsync(cancellationToken);
        return Results.Ok(new { notificationId, status = notification.Status, request.RunAtUtc });
    }

    private static async Task<IResult> ListSchedulesAsync(IApplicationReadDbContext readContext, CancellationToken cancellationToken)
    {
        List<object> items = await (
            from schedule in readContext.NotificationSchedules
            join notification in readContext.NotificationMessages on schedule.NotificationId equals notification.Id
            where !schedule.IsCancelled
            orderby schedule.RunAtUtc
            select new
            {
                schedule.Id,
                schedule.NotificationId,
                schedule.RunAtUtc,
                schedule.RuleName,
                notification.Channel,
                notification.Status
            })
            .Cast<object>()
            .ToListAsync(cancellationToken);

        return Results.Ok(new { total = items.Count, items });
    }

    private static async Task<IResult> DeleteScheduleAsync(Guid scheduleId, IApplicationDbContext writeContext, CancellationToken cancellationToken)
    {
        NotificationSchedule? schedule = await writeContext.NotificationSchedules.SingleOrDefaultAsync(x => x.Id == scheduleId, cancellationToken);
        if (schedule is null)
        {
            return Results.NotFound();
        }

        schedule.IsCancelled = true;
        NotificationMessage? notification = await writeContext.NotificationMessages.SingleOrDefaultAsync(x => x.Id == schedule.NotificationId, cancellationToken);
        if (notification is not null && notification.Status == NotificationStatus.Scheduled)
        {
            notification.Status = NotificationStatus.Pending;
            notification.ScheduledAtUtc = null;
        }

        await writeContext.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> UpsertPermissionAsync(Guid notificationId, PermissionRequest request, IApplicationDbContext writeContext, CancellationToken cancellationToken)
    {
        string subjectType = NormalizeSubjectType(request.SubjectType);
        if (subjectType.Length == 0 || string.IsNullOrWhiteSpace(request.SubjectValue))
        {
            return Results.BadRequest(new { message = "Invalid permission payload." });
        }

        string subjectValue = request.SubjectValue.Trim();
        if (subjectType == SubjectTypeUser && Guid.TryParse(subjectValue, out Guid userId))
        {
            subjectValue = userId.ToString("N");
        }

        NotificationPermissionEntry? existing = await writeContext.NotificationPermissionEntries
            .SingleOrDefaultAsync(
                x => x.NotificationId == notificationId &&
                     x.SubjectType == subjectType &&
                     x.SubjectValue == subjectValue,
                cancellationToken);

        if (existing is null)
        {
            existing = new NotificationPermissionEntry
            {
                Id = Guid.NewGuid(),
                NotificationId = notificationId,
                SubjectType = subjectType,
                SubjectValue = subjectValue,
                CreatedAtUtc = DateTime.UtcNow
            };
            writeContext.NotificationPermissionEntries.Add(existing);
        }

        existing.CanRead = request.CanRead;
        existing.CanManage = request.CanManage;
        await writeContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(new { existing.Id, existing.NotificationId, existing.SubjectType, existing.SubjectValue, existing.CanRead, existing.CanManage });
    }

    private static async Task<IResult> GetPermissionsAsync(Guid notificationId, IApplicationReadDbContext readContext, CancellationToken cancellationToken)
    {
        List<object> items = await readContext.NotificationPermissionEntries
            .Where(x => x.NotificationId == notificationId)
            .Select(x => new { x.Id, x.SubjectType, x.SubjectValue, x.CanRead, x.CanManage })
            .Cast<object>()
            .ToListAsync(cancellationToken);

        return Results.Ok(new { notificationId, items });
    }

    private static async Task<IResult> ArchiveAsync(Guid id, IApplicationDbContext writeContext, CancellationToken cancellationToken)
    {
        NotificationMessage? notification = await writeContext.NotificationMessages.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (notification is null)
        {
            return Results.NotFound();
        }

        notification.IsArchived = true;
        notification.ArchivedAtUtc = DateTime.UtcNow;
        await writeContext.SaveChangesAsync(cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> ReportSummaryAsync(DateTime? from, DateTime? to, IApplicationReadDbContext readContext, CancellationToken cancellationToken)
    {
        DateTime dateFrom = from ?? DateTime.UtcNow.AddDays(-7);
        DateTime dateTo = to ?? DateTime.UtcNow;

        IQueryable<NotificationMessage> query = readContext.NotificationMessages
            .Where(x => x.CreatedAtUtc >= dateFrom && x.CreatedAtUtc <= dateTo);

        int total = await query.CountAsync(cancellationToken);
        int delivered = await query.CountAsync(x => x.Status == NotificationStatus.Delivered, cancellationToken);
        int failed = await query.CountAsync(x => x.Status == NotificationStatus.Failed, cancellationToken);
        int pending = await query.CountAsync(
            x => x.Status == NotificationStatus.Pending || x.Status == NotificationStatus.Scheduled,
            cancellationToken);

        List<object> byChannel = await query
            .GroupBy(x => x.Channel)
            .Select(x => new { channel = x.Key, total = x.Count() })
            .Cast<object>()
            .ToListAsync(cancellationToken);

        return Results.Ok(new
        {
            from = dateFrom,
            to = dateTo,
            total,
            delivered,
            failed,
            pending,
            byChannel
        });
    }

    private static async Task<IResult> ReportDetailsAsync(
        DateTime? from,
        DateTime? to,
        NotificationChannel? channel,
        NotificationStatus? status,
        int page,
        int pageSize,
        IApplicationReadDbContext readContext,
        CancellationToken cancellationToken)
    {
        DateTime dateFrom = from ?? DateTime.UtcNow.AddDays(-7);
        DateTime dateTo = to ?? DateTime.UtcNow;

        IQueryable<NotificationMessage> query = readContext.NotificationMessages
            .Where(x => x.CreatedAtUtc >= dateFrom && x.CreatedAtUtc <= dateTo);

        if (channel.HasValue)
        {
            query = query.Where(x => x.Channel == channel.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        (int normalizedPage, int normalizedPageSize) = Application.Abstractions.Data.QueryableExtensions.NormalizePaging(page, pageSize, 50, 200);
        int total = await query.CountAsync(cancellationToken);
        List<object> items = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip((normalizedPage - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .Select(x => new
            {
                x.Id,
                x.Channel,
                x.Priority,
                x.Status,
                x.Subject,
                x.RetryCount,
                x.CreatedAtUtc,
                x.SentAtUtc,
                x.DeliveredAtUtc,
                x.LastError
            })
            .Cast<object>()
            .ToListAsync(cancellationToken);

        return Results.Ok(new { page = normalizedPage, pageSize = normalizedPageSize, total, items });
    }

    private static async Task<(string Subject, string Body)> ResolveContentAsync(
        IApplicationDbContext writeContext,
        CreateNotificationRequest request,
        CancellationToken cancellationToken)
    {
        if (!request.TemplateId.HasValue)
        {
            return (
                string.IsNullOrWhiteSpace(request.Subject) ? "(no-subject)" : request.Subject.Trim(),
                string.IsNullOrWhiteSpace(request.Body) ? string.Empty : request.Body);
        }

        NotificationTemplate? template = await writeContext.NotificationTemplates
            .SingleOrDefaultAsync(
                x => x.Id == request.TemplateId.Value &&
                     !x.IsDeleted &&
                     x.Channel == request.Channel &&
                     x.Language == request.Language,
                cancellationToken);

        if (template is null)
        {
            return (
                string.IsNullOrWhiteSpace(request.Subject) ? "(template-not-found)" : request.Subject.Trim(),
                string.IsNullOrWhiteSpace(request.Body) ? string.Empty : request.Body);
        }

        return (template.SubjectTemplate, template.BodyTemplate);
    }

    private static async Task<bool> CanReadAsync(
        NotificationMessage notification,
        Guid userId,
        IApplicationReadDbContext readContext,
        CancellationToken cancellationToken)
    {
        if (notification.CreatedByUserId == userId || await IsAdminAsync(userId, readContext, cancellationToken))
        {
            return true;
        }

        string uid = userId.ToString("N");
        return await readContext.NotificationPermissionEntries.AnyAsync(
            x => x.NotificationId == notification.Id &&
                 (x.SubjectType == SubjectTypeUser && x.SubjectValue == uid ||
                  x.SubjectType == SubjectTypeRole && UserRoleNames(readContext, userId).Contains(x.SubjectValue)) &&
                 x.CanRead,
            cancellationToken);
    }

    private static IQueryable<string> UserRoleNames(IApplicationReadDbContext readContext, Guid userId)
    {
        return from ur in readContext.UserRoles
               join role in readContext.Roles on ur.RoleId equals role.Id
               where ur.UserId == userId
               select role.Name;
    }

    private static async Task<bool> IsAdminAsync(Guid userId, IApplicationReadDbContext readContext, CancellationToken cancellationToken)
    {
        return await (from ur in readContext.UserRoles
                      join role in readContext.Roles on ur.RoleId equals role.Id
                      where ur.UserId == userId && role.Name == "admin"
                      select ur.UserId)
            .AnyAsync(cancellationToken);
    }

    private static string NormalizeSubjectType(string value)
    {
        if (string.Equals(value, SubjectTypeUser, StringComparison.OrdinalIgnoreCase))
        {
            return SubjectTypeUser;
        }

        if (string.Equals(value, SubjectTypeRole, StringComparison.OrdinalIgnoreCase))
        {
            return SubjectTypeRole;
        }

        return string.Empty;
    }
}
