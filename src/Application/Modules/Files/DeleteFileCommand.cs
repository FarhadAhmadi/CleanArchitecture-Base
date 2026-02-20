using Application.Abstractions.Files;
using Application.Abstractions.Messaging;

namespace Application.Modules.Files;

public sealed record DeleteFileCommand(Guid FileId, HttpContext HttpContext) : ICommand<IResult>;
internal sealed class DeleteFileCommandHandler(IFileUseCaseService service) : ResultWrappingCommandHandler<DeleteFileCommand>
{
    protected override async Task<IResult> HandleCore(DeleteFileCommand command, CancellationToken cancellationToken) =>
        await service.DeleteAsync(command.FileId, command.HttpContext, cancellationToken);
}





