using Application.Abstractions.Files;
using Application.Abstractions.Messaging;

namespace Application.Modules.Files;

public sealed record SetFileEncryptedCommand(Guid FileId) : ICommand<IResult>;
internal sealed class SetFileEncryptedCommandHandler(IFileUseCaseService service) : ResultWrappingCommandHandler<SetFileEncryptedCommand>
{
    protected override async Task<IResult> HandleCore(SetFileEncryptedCommand command, CancellationToken cancellationToken) =>
        await service.SetEncryptedAsync(command.FileId, cancellationToken);
}





