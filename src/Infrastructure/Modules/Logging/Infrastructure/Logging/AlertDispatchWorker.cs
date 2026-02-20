using Domain.Logging;
using Domain.Modules.Notifications;
using Domain.Notifications;
using Domain.Users;
using Infrastructure.Authorization;
using Infrastructure.Database;
using Infrastructure.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace Infrastructure.Logging;

internal sealed class AlertDispatchWorker(
    IAlertDispatchQueue queue,
    IServiceScopeFactory scopeFactory,
    AuthorizationBootstrapOptions authorizationOptions,
    NotificationOptions notificationOptions,
    NotificationSensitiveDataProtector protector,
    ILogger<AlertDispatchWorker> logger) : BackgroundService
{
    private readonly ResiliencePipeline _retryPipeline = new ResiliencePipelineBuilder()
        .AddRetry(new RetryStrategyOptions
        {
            ShouldHandle = new PredicateBuilder().Handle<Exception>(),
            MaxRetryAttempts = 3,
            BackoffType = DelayBackoffType.Exponential,
            Delay = TimeSpan.FromMilliseconds(250)
        })
        .Build();

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            Guid incidentId = await queue.DequeueAsync(stoppingToken);

            using IServiceScope scope = scopeFactory.CreateScope();
            ApplicationDbContext db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            NotificationTemplateRenderer templateRenderer = scope.ServiceProvider.GetRequiredService<NotificationTemplateRenderer>();

            AlertIncident? incident = await db.AlertIncidents.SingleOrDefaultAsync(x => x.Id == incidentId, stoppingToken);
            if (incident is null)
            {
                continue;
            }

            incident.Status = "Dispatching";
            await db.SaveChangesAsync(stoppingToken);

            AlertRule? rule = await db.AlertRules.SingleOrDefaultAsync(x => x.Id == incident.RuleId, stoppingToken);
            LogEvent? triggerEvent = await db.LogEvents.SingleOrDefaultAsync(x => x.Id == incident.TriggerEventId, stoppingToken);
            if (rule is null || triggerEvent is null)
            {
                incident.Status = "Failed";
                incident.LastError = "Alert incident dependencies are missing.";
                await db.SaveChangesAsync(stoppingToken);
                continue;
            }

            try
            {
                await _retryPipeline.ExecuteAsync(async token =>
                {
                    int notificationCount = await QueueAdminNotificationsAsync(
                        db,
                        templateRenderer,
                        incident,
                        rule,
                        triggerEvent,
                        token);

                    incident.Status = "Delivered";
                    incident.LastError = null;
                    await db.SaveChangesAsync(token);

                    logger.LogWarning(
                        "Alert incident delivered and queued notifications. IncidentId={IncidentId} RuleId={RuleId} NotificationCount={NotificationCount}",
                        incident.Id,
                        rule.Id,
                        notificationCount);
                }, stoppingToken);
            }
            catch (Exception ex)
            {
                incident.RetryCount++;
                incident.Status = "Failed";
                incident.LastError = ex.Message;
                incident.NextRetryAtUtc = DateTime.UtcNow.AddSeconds(Math.Pow(2, Math.Min(incident.RetryCount, 8)));
                await db.SaveChangesAsync(stoppingToken);
                logger.LogError(ex, "Alert dispatch failed for incident {IncidentId}", incidentId);
            }
        }
    }

    private async Task<int> QueueAdminNotificationsAsync(
        ApplicationDbContext db,
        NotificationTemplateRenderer templateRenderer,
        AlertIncident incident,
        AlertRule rule,
        LogEvent triggerEvent,
        CancellationToken cancellationToken)
    {
        List<User> adminUsers = await (
            from user in db.Users
            join userRole in db.UserRoles on user.Id equals userRole.UserId
            join role in db.Roles on userRole.RoleId equals role.Id
            where role.Name == authorizationOptions.AdminRoleName &&
                  user.Email != null &&
                  !string.IsNullOrWhiteSpace(user.Email)
            select user)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (adminUsers.Count == 0)
        {
            throw new InvalidOperationException("No admin user with email was found for alert dispatch.");
        }

        var variables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["AdminDisplayName"] = "Administrator",
            ["RuleName"] = rule.Name,
            ["IncidentId"] = incident.Id.ToString("N"),
            ["EventId"] = triggerEvent.Id.ToString("N"),
            ["Severity"] = triggerEvent.Level.ToString(),
            ["TriggeredAtUtc"] = incident.TriggeredAtUtc.ToString("O"),
            ["SourceService"] = triggerEvent.SourceService,
            ["SourceModule"] = triggerEvent.SourceModule,
            ["Message"] = triggerEvent.Message,
            ["TraceId"] = triggerEvent.TraceId,
            ["Outcome"] = triggerEvent.Outcome
        };

        RenderedNotificationTemplate? renderedTemplate = await templateRenderer.TryRenderAsync(
            NotificationTemplateCatalog.AlertIncidentEmail,
            "fa-IR",
            NotificationChannel.Email,
            variables,
            cancellationToken);

        string subject = renderedTemplate?.Subject ?? $"Alert triggered: {rule.Name}";
        string body = renderedTemplate?.Body ??
                      $"Rule={rule.Name}\nIncidentId={incident.Id:N}\nEventId={triggerEvent.Id:N}\nMessage={triggerEvent.Message}";

        foreach (User admin in adminUsers)
        {
            string email = admin.Email!;
            db.NotificationMessages.Add(new NotificationMessage
            {
                Id = Guid.NewGuid(),
                CreatedByUserId = admin.Id,
                Channel = NotificationChannel.Email,
                Priority = NotificationPriority.High,
                Status = NotificationStatus.Pending,
                RecipientEncrypted = protector.Protect(email),
                RecipientHash = NotificationSensitiveDataProtector.ComputeDeterministicHash(email),
                Subject = subject,
                Body = body,
                Language = "fa-IR",
                TemplateId = renderedTemplate?.TemplateId,
                CreatedAtUtc = DateTime.UtcNow,
                MaxRetryCount = Math.Max(1, notificationOptions.MaxRetries)
            });
        }

        return adminUsers.Count;
    }
}
