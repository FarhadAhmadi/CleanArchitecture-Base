using Application.Abstractions.Files;
using Application.Abstractions.Messaging;

namespace Application.Modules.Files;

public sealed record ShareFileCommand(Guid FileId, string? Mode, HttpContext HttpContext) : ICommand<IResult>;
internal sealed class ShareFileCommandHandler(IFileUseCaseService service) : ResultWrappingCommandHandler<ShareFileCommand>
{
    protected override async Task<IResult> HandleCore(ShareFileCommand command, CancellationToken cancellationToken) =>
        await service.ShareAsync(command.FileId, command.Mode, command.HttpContext, cancellationToken);
}





