using Application.Abstractions.Files;
using Application.Abstractions.Messaging;

namespace Application.Modules.Files;

public sealed record ScanFileCommand(ScanFileInput Request) : ICommand<IResult>;
internal sealed class ScanFileCommandHandler(IFileUseCaseService service) : ResultWrappingCommandHandler<ScanFileCommand>
{
    protected override async Task<IResult> HandleCore(ScanFileCommand command, CancellationToken cancellationToken) =>
        await service.ScanAsync(command.Request, cancellationToken);
}





