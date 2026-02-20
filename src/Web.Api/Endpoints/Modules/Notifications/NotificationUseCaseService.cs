using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Notifications;
using Domain.Modules.Notifications;
using Domain.Notifications;
using Infrastructure.Notifications;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Endpoints.Notifications;

internal sealed class NotificationUseCaseService(
    IUserContext userContext,
    IApplicationDbContext writeContext,
    IApplicationReadDbContext readContext,
    NotificationSensitiveDataProtector protector,
    NotificationOptions options) : INotificationUseCaseService
{
    private const string SubjectTypeUser = "User";
    private const string SubjectTypeRole = "Role";

    public async Task<IResult> CreateNotificationAsync(
        NotificationChannel channel,
        NotificationPriority priority,
        string recipient,
        string? subject,
        string? body,
        string language,
        Guid? templateId,
        DateTime? scheduledAtUtc,
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

        string normalizedRecipient = (recipient ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(normalizedRecipient))
        {
            return Results.BadRequest(new { message = "Recipient is required." });
        }

        (string resolvedSubject, string resolvedBody) = await ResolveContentAsync(templateId, channel, language, subject, body, cancellationToken);

        NotificationMessage message = new()
        {
            Id = Guid.NewGuid(),
            CreatedByUserId = userContext.UserId,
            Channel = channel,
            Priority = priority,
            Status = scheduledAtUtc.HasValue && scheduledAtUtc.Value > now
                ? NotificationStatus.Scheduled
                : NotificationStatus.Pending,
            RecipientEncrypted = protector.Protect(normalizedRecipient),
            RecipientHash = NotificationSensitiveDataProtector.ComputeDeterministicHash(normalizedRecipient),
            Subject = resolvedSubject,
            Body = resolvedBody,
            Language = string.IsNullOrWhiteSpace(language) ? "fa-IR" : language.Trim(),
            TemplateId = templateId,
            CreatedAtUtc = now,
            ScheduledAtUtc = scheduledAtUtc,
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

    public async Task<IResult> GetNotificationAsync(Guid notificationId, CancellationToken cancellationToken)
    {
        NotificationMessage? notification = await readContext.NotificationMessages
            .SingleOrDefaultAsync(x => x.Id == notificationId, cancellationToken);

        if (notification is null)
        {
            return Results.NotFound();
        }

        if (!await CanReadAsync(notification, userContext.UserId, cancellationToken))
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

    public async Task<IResult> ListNotificationsAsync(
        int? page,
        int? pageIndex,
        int? pageSize,
        NotificationChannel? channel,
        NotificationStatus? status,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken)
    {
        IQueryable<NotificationMessage> query = readContext.NotificationMessages;
        bool isAdmin = await IsAdminAsync(userContext.UserId, cancellationToken);
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

        int normalizedPage = pageIndex ?? page ?? 1;
        int normalizedPageSize = pageSize ?? 50;
        (normalizedPage, normalizedPageSize) = Application.Abstractions.Data.QueryableExtensions.NormalizePaging(normalizedPage, normalizedPageSize, 50, 200);
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

    public async Task<IResult> CreateTemplateAsync(
        string name,
        NotificationChannel channel,
        string language,
        string subjectTemplate,
        string bodyTemplate,
        CancellationToken cancellationToken)
    {
        NotificationTemplate template = new()
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Channel = channel,
            Language = string.IsNullOrWhiteSpace(language) ? "fa-IR" : language.Trim(),
            SubjectTemplate = subjectTemplate,
            BodyTemplate = bodyTemplate,
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

    public async Task<IResult> GetTemplateAsync(Guid templateId, CancellationToken cancellationToken)
    {
        NotificationTemplate? template = await readContext.NotificationTemplates
            .SingleOrDefaultAsync(x => x.Id == templateId && !x.IsDeleted, cancellationToken);
        return template is null ? Results.NotFound() : Results.Ok(template);
    }

    public async Task<IResult> UpdateTemplateAsync(Guid templateId, string subjectTemplate, string bodyTemplate, CancellationToken cancellationToken)
    {
        NotificationTemplate? template = await writeContext.NotificationTemplates
            .SingleOrDefaultAsync(x => x.Id == templateId && !x.IsDeleted, cancellationToken);
        if (template is null)
        {
            return Results.NotFound();
        }

        template.SubjectTemplate = subjectTemplate;
        template.BodyTemplate = bodyTemplate;
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

    public async Task<IResult> DeleteTemplateAsync(Guid templateId, CancellationToken cancellationToken)
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

    public async Task<IResult> ListTemplatesAsync(string? language, NotificationChannel? channel, CancellationToken cancellationToken)
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

    public async Task<IResult> ScheduleNotificationAsync(Guid notificationId, DateTime runAtUtc, string? ruleName, CancellationToken cancellationToken)
    {
        NotificationMessage? notification = await writeContext.NotificationMessages.SingleOrDefaultAsync(x => x.Id == notificationId, cancellationToken);
        if (notification is null)
        {
            return Results.NotFound();
        }

        notification.Status = NotificationStatus.Scheduled;
        notification.ScheduledAtUtc = runAtUtc;
        notification.NextRetryAtUtc = runAtUtc;

        NotificationSchedule? existing = await writeContext.NotificationSchedules
            .SingleOrDefaultAsync(x => x.NotificationId == notificationId && !x.IsCancelled, cancellationToken);

        if (existing is null)
        {
            writeContext.NotificationSchedules.Add(new NotificationSchedule
            {
                Id = Guid.NewGuid(),
                NotificationId = notificationId,
                RunAtUtc = runAtUtc,
                RuleName = ruleName,
                CreatedAtUtc = DateTime.UtcNow
            });
        }
        else
        {
            existing.RunAtUtc = runAtUtc;
            existing.RuleName = ruleName;
            existing.IsCancelled = false;
        }

        await writeContext.SaveChangesAsync(cancellationToken);
        return Results.Ok(new { notificationId, status = notification.Status, runAtUtc });
    }

    public async Task<IResult> ListSchedulesAsync(CancellationToken cancellationToken)
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

    public async Task<IResult> DeleteScheduleAsync(Guid scheduleId, CancellationToken cancellationToken)
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

    public async Task<IResult> UpsertPermissionAsync(
        Guid notificationId,
        string subjectType,
        string subjectValue,
        bool canRead,
        bool canManage,
        CancellationToken cancellationToken)
    {
        string normalizedSubjectType = NormalizeSubjectType(subjectType);
        if (normalizedSubjectType.Length == 0 || string.IsNullOrWhiteSpace(subjectValue))
        {
            return Results.BadRequest(new { message = "Invalid permission payload." });
        }

        string normalizedSubjectValue = subjectValue.Trim();
        if (normalizedSubjectType == SubjectTypeUser && Guid.TryParse(normalizedSubjectValue, out Guid userId))
        {
            normalizedSubjectValue = userId.ToString("N");
        }

        NotificationPermissionEntry? existing = await writeContext.NotificationPermissionEntries
            .SingleOrDefaultAsync(
                x => x.NotificationId == notificationId &&
                     x.SubjectType == normalizedSubjectType &&
                     x.SubjectValue == normalizedSubjectValue,
                cancellationToken);

        if (existing is null)
        {
            existing = new NotificationPermissionEntry
            {
                Id = Guid.NewGuid(),
                NotificationId = notificationId,
                SubjectType = normalizedSubjectType,
                SubjectValue = normalizedSubjectValue,
                CreatedAtUtc = DateTime.UtcNow
            };
            writeContext.NotificationPermissionEntries.Add(existing);
        }

        existing.CanRead = canRead;
        existing.CanManage = canManage;
        await writeContext.SaveChangesAsync(cancellationToken);

        return Results.Ok(new { existing.Id, existing.NotificationId, existing.SubjectType, existing.SubjectValue, existing.CanRead, existing.CanManage });
    }

    public async Task<IResult> GetPermissionsAsync(Guid notificationId, CancellationToken cancellationToken)
    {
        List<object> items = await readContext.NotificationPermissionEntries
            .Where(x => x.NotificationId == notificationId)
            .Select(x => new { x.Id, x.SubjectType, x.SubjectValue, x.CanRead, x.CanManage })
            .Cast<object>()
            .ToListAsync(cancellationToken);

        return Results.Ok(new { notificationId, items });
    }

    public async Task<IResult> ArchiveAsync(Guid id, CancellationToken cancellationToken)
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

    public async Task<IResult> ReportSummaryAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken)
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

    public async Task<IResult> ReportDetailsAsync(
        int? page,
        int? pageIndex,
        int? pageSize,
        DateTime? from,
        DateTime? to,
        NotificationChannel? channel,
        NotificationStatus? status,
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

        int normalizedPage = pageIndex ?? page ?? 1;
        int normalizedPageSize = pageSize ?? 50;
        (normalizedPage, normalizedPageSize) = Application.Abstractions.Data.QueryableExtensions.NormalizePaging(normalizedPage, normalizedPageSize, 50, 200);
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

    private async Task<(string Subject, string Body)> ResolveContentAsync(
        Guid? templateId,
        NotificationChannel channel,
        string language,
        string? subject,
        string? body,
        CancellationToken cancellationToken)
    {
        if (!templateId.HasValue)
        {
            return (
                string.IsNullOrWhiteSpace(subject) ? "(no-subject)" : subject.Trim(),
                string.IsNullOrWhiteSpace(body) ? string.Empty : body);
        }

        NotificationTemplate? template = await writeContext.NotificationTemplates
            .SingleOrDefaultAsync(
                x => x.Id == templateId.Value &&
                     !x.IsDeleted &&
                     x.Channel == channel &&
                     x.Language == language,
                cancellationToken);

        if (template is null)
        {
            return (
                string.IsNullOrWhiteSpace(subject) ? "(template-not-found)" : subject.Trim(),
                string.IsNullOrWhiteSpace(body) ? string.Empty : body);
        }

        return (template.SubjectTemplate, template.BodyTemplate);
    }

    private async Task<bool> CanReadAsync(NotificationMessage notification, Guid currentUserId, CancellationToken cancellationToken)
    {
        if (notification.CreatedByUserId == currentUserId || await IsAdminAsync(currentUserId, cancellationToken))
        {
            return true;
        }

        string uid = currentUserId.ToString("N");
        return await readContext.NotificationPermissionEntries.AnyAsync(
            x => x.NotificationId == notification.Id &&
                 (x.SubjectType == SubjectTypeUser && x.SubjectValue == uid ||
                  x.SubjectType == SubjectTypeRole && UserRoleNames(currentUserId).Contains(x.SubjectValue)) &&
                 x.CanRead,
            cancellationToken);
    }

    private IQueryable<string> UserRoleNames(Guid currentUserId)
    {
        return from ur in readContext.UserRoles
               join role in readContext.Roles on ur.RoleId equals role.Id
               where ur.UserId == currentUserId
               select role.Name;
    }

    private async Task<bool> IsAdminAsync(Guid currentUserId, CancellationToken cancellationToken)
    {
        return await (from ur in readContext.UserRoles
                      join role in readContext.Roles on ur.RoleId equals role.Id
                      where ur.UserId == currentUserId && role.Name == "admin"
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

