using Application.Abstractions.Files;
using Application.Abstractions.Messaging;

namespace Application.Modules.Files;

public sealed record UpsertFilePermissionCommand(Guid FileId, UpsertFilePermissionInput Request) : ICommand<IResult>;
internal sealed class UpsertFilePermissionCommandHandler(IFileUseCaseService service) : ResultWrappingCommandHandler<UpsertFilePermissionCommand>
{
    protected override async Task<IResult> HandleCore(UpsertFilePermissionCommand command, CancellationToken cancellationToken) =>
        await service.UpsertPermissionAsync(command.FileId, command.Request, cancellationToken);
}





