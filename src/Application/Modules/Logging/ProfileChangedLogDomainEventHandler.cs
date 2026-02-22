using Application.Abstractions.Logging;
using Domain.Logging;
using Domain.Profiles;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Logging;

internal sealed class ProfileChangedLogDomainEventHandler(
    IApplicationLogWriter logWriter,
    ILogger<ProfileChangedLogDomainEventHandler> logger,
    IDateTimeProvider dateTimeProvider)
    : IDomainEventHandler<UserProfileChangedDomainEvent>
{
    public async Task Handle(UserProfileChangedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        DateTime now = dateTimeProvider.UtcNow;
        string traceId = domainEvent.ProfileId.ToString("N");
        string actorId = domainEvent.UserId.ToString("N");
        Guid logId = CreateDeterministicGuid($"profiles.log|{traceId}|{domainEvent.ChangeType}|{domainEvent.CompletenessScore}");

        bool inserted = await logWriter.TryWriteAsync(
            new ApplicationLogEntry(
                Id: logId,
                TimestampUtc: now,
                Level: LogLevelType.Info,
                Message: $"Profile changed ({domainEvent.ChangeType})",
                SourceService: "Web.Api",
                SourceModule: "Profiles",
                TraceId: traceId,
                ActorType: "User",
                ActorId: actorId,
                Outcome: "Success",
                TagsCsv: "profile,domain-event"),
            cancellationToken);

        if (logger.IsEnabled(LogLevel.Debug))
        {
            logger.LogDebug(
                "ProfileChanged log fanout handled. ProfileId={ProfileId} LogId={LogId} Inserted={Inserted}",
                domainEvent.ProfileId,
                logId,
                inserted);
        }
    }

    private static Guid CreateDeterministicGuid(string value)
    {
        byte[] hash = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(value));
        Span<byte> bytes = stackalloc byte[16];
        hash.AsSpan(0, 16).CopyTo(bytes);
        return new Guid(bytes);
    }
}
