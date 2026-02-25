using Application.Abstractions.Scheduler;
using Domain.Scheduler;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Scheduler;

internal sealed class SchedulerExecutionService(
    IEnumerable<IScheduledJobHandler> handlers,
    ISchedulerPayloadValidator payloadValidator,
    ILogger<SchedulerExecutionService> logger) : ISchedulerExecutionService
{
    private readonly Dictionary<JobType, IScheduledJobHandler> _handlerByType = handlers
        .GroupBy(x => x.JobType)
        .ToDictionary(x => x.Key, x => x.First());

    public async Task<SchedulerExecutionResult> ExecuteAsync(SchedulerExecutionRequest request, CancellationToken cancellationToken)
    {
        SchedulerPayloadValidationResult validation = payloadValidator.Validate(request.Job.Type, request.Job.PayloadJson);
        if (!validation.IsValid)
        {
            string error = validation.Error ?? $"Payload is invalid for job type '{request.Job.Type}'.";
            logger.LogWarning(
                "Scheduler execution rejected by payload validation. JobId={JobId} Type={Type} Error={Error}",
                request.Job.Id,
                request.Job.Type,
                error);
            return new SchedulerExecutionResult(JobExecutionStatus.Failed, 0, error, false, false);
        }

        request.Job.PayloadJson = validation.NormalizedPayloadJson;

        if (!_handlerByType.TryGetValue(request.Job.Type, out IScheduledJobHandler? handler))
        {
            string error = $"No handler registered for job type '{request.Job.Type}'.";
            logger.LogWarning("Scheduler execution skipped. JobId={JobId} Type={Type} Reason={Reason}", request.Job.Id, request.Job.Type, error);
            return new SchedulerExecutionResult(JobExecutionStatus.Failed, 0, error, false, false);
        }

        DateTime startedAtUtc = DateTime.UtcNow;
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(Math.Max(1, request.Job.MaxExecutionSeconds)));

        try
        {
            await handler.ExecuteAsync(request.Job, timeoutCts.Token);
            int durationMs = (int)Math.Max(0, (DateTime.UtcNow - startedAtUtc).TotalMilliseconds);
            return new SchedulerExecutionResult(JobExecutionStatus.Succeeded, durationMs, null, false, false);
        }
        catch (OperationCanceledException ex) when (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            int durationMs = (int)Math.Max(0, (DateTime.UtcNow - startedAtUtc).TotalMilliseconds);
            logger.LogWarning(
                ex,
                "Scheduler execution timed out. JobId={JobId} Type={Type} TimeoutSeconds={TimeoutSeconds}",
                request.Job.Id,
                request.Job.Type,
                request.Job.MaxExecutionSeconds);
            return new SchedulerExecutionResult(JobExecutionStatus.TimedOut, durationMs, $"Execution timed out after {request.Job.MaxExecutionSeconds} seconds.", true, false);
        }
        catch (OperationCanceledException ex)
        {
            int durationMs = (int)Math.Max(0, (DateTime.UtcNow - startedAtUtc).TotalMilliseconds);
            logger.LogWarning(ex, "Scheduler execution canceled. JobId={JobId} Type={Type}", request.Job.Id, request.Job.Type);
            return new SchedulerExecutionResult(JobExecutionStatus.Canceled, durationMs, "Execution canceled.", false, true);
        }
        catch (Exception ex)
        {
            int durationMs = (int)Math.Max(0, (DateTime.UtcNow - startedAtUtc).TotalMilliseconds);
            logger.LogError(ex, "Scheduler execution failed. JobId={JobId} Type={Type}", request.Job.Id, request.Job.Type);
            return new SchedulerExecutionResult(JobExecutionStatus.Failed, durationMs, ex.Message, false, false);
        }
    }
}
