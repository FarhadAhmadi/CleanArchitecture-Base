using Application.Abstractions.Messaging;
using Application.Abstractions.Observability;

namespace Application.Observability;

public sealed record ReplayFailedInboxCommand(int Take) : ICommand<IResult>;
internal sealed class ReplayFailedInboxCommandHandler(IOrchestrationReplayService replayService) : ResultWrappingCommandHandler<ReplayFailedInboxCommand>
{
    protected override async Task<IResult> HandleCore(ReplayFailedInboxCommand command, CancellationToken cancellationToken)
    {
        int replayed = await replayService.ReplayFailedInboxAsync(command.Take, cancellationToken);
        return Results.Ok(new { target = "inbox", replayed });
    }
}




