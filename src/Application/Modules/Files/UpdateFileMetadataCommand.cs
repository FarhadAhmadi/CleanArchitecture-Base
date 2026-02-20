using Application.Abstractions.Files;
using Application.Abstractions.Messaging;

namespace Application.Modules.Files;

public sealed record UpdateFileMetadataCommand(Guid FileId, UpdateFileMetadataInput Request) : ICommand<IResult>;
internal sealed class UpdateFileMetadataCommandHandler(IFileUseCaseService service) : ResultWrappingCommandHandler<UpdateFileMetadataCommand>
{
    protected override async Task<IResult> HandleCore(UpdateFileMetadataCommand command, CancellationToken cancellationToken) =>
        await service.UpdateMetadataAsync(command.FileId, command.Request, cancellationToken);
}





