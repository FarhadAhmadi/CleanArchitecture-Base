using Application.Abstractions.Messaging;
using Application.Abstractions.Observability;

namespace Application.Observability;

public sealed record ReplayFailedOutboxCommand(int Take) : ICommand<IResult>;
internal sealed class ReplayFailedOutboxCommandHandler(IOrchestrationReplayService replayService) : ResultWrappingCommandHandler<ReplayFailedOutboxCommand>
{
    protected override async Task<IResult> HandleCore(ReplayFailedOutboxCommand command, CancellationToken cancellationToken)
    {
        int replayed = await replayService.ReplayFailedOutboxAsync(command.Take, cancellationToken);
        return Results.Ok(new { target = "outbox", replayed });
    }
}




