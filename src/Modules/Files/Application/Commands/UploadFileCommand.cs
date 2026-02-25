using Application.Abstractions.Files;
using Application.Abstractions.Messaging;

namespace Application.Modules.Files;

public sealed record UploadFileCommand(UploadFileInput Request, HttpContext HttpContext) : ICommand<IResult>;
internal sealed class UploadFileCommandHandler(IFileUseCaseService service) : ResultWrappingCommandHandler<UploadFileCommand>
{
    protected override async Task<IResult> HandleCore(UploadFileCommand command, CancellationToken cancellationToken) =>
        await service.UploadAsync(command.Request, command.HttpContext, cancellationToken);
}





