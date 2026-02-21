using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Application.Abstractions.Scheduler;
using Domain.Modules.Notifications;
using Domain.Notifications;
using Domain.Modules.Scheduler;
using Domain.Scheduler;
using Infrastructure.Database;
using Infrastructure.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Scheduler;

internal sealed class NotificationDispatchProbeJobHandler(
    ApplicationDbContext dbContext,
    NotificationSensitiveDataProtector protector,
    ILogger<NotificationDispatchProbeJobHandler> logger) : IScheduledJobHandler
{
    public JobType JobType => JobType.NotificationDispatchProbe;

    public async Task ExecuteAsync(ScheduledJob job, CancellationToken cancellationToken)
    {
        ProbePayload payload = ReadPayload(job.PayloadJson);
        Guid createdByUserId = await dbContext.Users
            .OrderBy(x => x.AuditCreatedAtUtc)
            .Select(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (createdByUserId == Guid.Empty)
        {
            throw new InvalidOperationException("NotificationDispatchProbe requires at least one existing user as CreatedByUserId.");
        }

        string recipient = string.IsNullOrWhiteSpace(payload.Recipient)
            ? "scheduler-probe@local.test"
            : payload.Recipient.Trim();

        var message = new NotificationMessage
        {
            Id = Guid.NewGuid(),
            CreatedByUserId = createdByUserId,
            Channel = NotificationChannel.InApp,
            Priority = NotificationPriority.Medium,
            Status = NotificationStatus.Pending,
            RecipientEncrypted = protector.Protect(recipient),
            RecipientHash = ComputeSha256(recipient),
            Subject = payload.Subject ?? "Scheduler Probe",
            Body = payload.Body ?? $"Scheduler probe executed at {DateTime.UtcNow:O}.",
            Language = "en-US",
            CreatedAtUtc = DateTime.UtcNow,
            MaxRetryCount = 3
        };

        dbContext.NotificationMessages.Add(message);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "NotificationDispatchProbe queued notification. JobId={JobId} NotificationId={NotificationId}",
                job.Id,
                message.Id);
        }
    }

    private static ProbePayload ReadPayload(string? payloadJson)
    {
        if (string.IsNullOrWhiteSpace(payloadJson))
        {
            return new ProbePayload(null, null, null);
        }

        try
        {
            return JsonSerializer.Deserialize<ProbePayload>(payloadJson) ?? new ProbePayload(null, null, null);
        }
        catch (JsonException)
        {
            return new ProbePayload(null, null, null);
        }
    }

    private static string ComputeSha256(string value)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(value);
        return Convert.ToHexString(SHA256.HashData(bytes));
    }

    private sealed record ProbePayload(string? Recipient, string? Subject, string? Body);
}
