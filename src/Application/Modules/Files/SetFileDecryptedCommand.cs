using Application.Abstractions.Files;
using Application.Abstractions.Messaging;

namespace Application.Modules.Files;

public sealed record SetFileDecryptedCommand(Guid FileId) : ICommand<IResult>;
internal sealed class SetFileDecryptedCommandHandler(IFileUseCaseService service) : ResultWrappingCommandHandler<SetFileDecryptedCommand>
{
    protected override async Task<IResult> HandleCore(SetFileDecryptedCommand command, CancellationToken cancellationToken) =>
        await service.SetDecryptedAsync(command.FileId, cancellationToken);
}





