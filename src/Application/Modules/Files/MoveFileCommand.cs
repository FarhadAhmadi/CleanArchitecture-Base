using Application.Abstractions.Files;
using Application.Abstractions.Messaging;

namespace Application.Modules.Files;

public sealed record MoveFileCommand(Guid FileId, MoveFileInput Request) : ICommand<IResult>;
internal sealed class MoveFileCommandHandler(IFileUseCaseService service) : ResultWrappingCommandHandler<MoveFileCommand>
{
    protected override async Task<IResult> HandleCore(MoveFileCommand command, CancellationToken cancellationToken) =>
        await service.MoveAsync(command.FileId, command.Request, cancellationToken);
}





