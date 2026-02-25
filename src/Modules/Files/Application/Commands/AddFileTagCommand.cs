using Application.Abstractions.Files;
using Application.Abstractions.Messaging;

namespace Application.Modules.Files;

public sealed record AddFileTagCommand(Guid FileId, AddFileTagInput Request) : ICommand<IResult>;
internal sealed class AddFileTagCommandHandler(IFileUseCaseService service) : ResultWrappingCommandHandler<AddFileTagCommand>
{
    protected override async Task<IResult> HandleCore(AddFileTagCommand command, CancellationToken cancellationToken) =>
        await service.AddTagAsync(command.FileId, command.Request, cancellationToken);
}





