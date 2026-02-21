using Application.Abstractions.Scheduler;
using Domain.Modules.Scheduler;
using Domain.Scheduler;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Infrastructure.Scheduler;

internal sealed class GenericNoOpScheduledJobHandler(ILogger<GenericNoOpScheduledJobHandler> logger) : IScheduledJobHandler
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public JobType JobType => JobType.GenericNoOp;

    public async Task ExecuteAsync(ScheduledJob job, CancellationToken cancellationToken)
    {
        NoOpPayload payload = ReadPayload(job.PayloadJson);
        int delaySeconds = payload.DelaySeconds
            ?? (string.Equals(payload.Note, "slow", StringComparison.OrdinalIgnoreCase)
                ? job.MaxExecutionSeconds + 1
                : 0);

        if (delaySeconds > 0)
        {
            await Task.Delay(TimeSpan.FromSeconds(Math.Min(delaySeconds, 600)), cancellationToken);
        }

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation(
                "Executed GenericNoOp scheduler job. JobId={JobId} Name={Name} DelaySeconds={DelaySeconds}",
                job.Id,
                job.Name,
                delaySeconds);
        }
    }

    private static NoOpPayload ReadPayload(string? payloadJson)
    {
        if (string.IsNullOrWhiteSpace(payloadJson))
        {
            return new NoOpPayload(null, null);
        }

        try
        {
            return JsonSerializer.Deserialize<NoOpPayload>(payloadJson, SerializerOptions) ?? new NoOpPayload(null, null);
        }
        catch (JsonException)
        {
            return new NoOpPayload(null, null);
        }
    }

    private sealed record NoOpPayload(string? Note, int? DelaySeconds);
}
