using Application.Abstractions.Files;
using Application.Abstractions.Messaging;

namespace Application.Modules.Files;

public sealed record ValidateFileCommand(ValidateFileInput Request) : ICommand<IResult>;
internal sealed class ValidateFileCommandHandler(IFileUseCaseService service) : ResultWrappingCommandHandler<ValidateFileCommand>
{
    protected override async Task<IResult> HandleCore(ValidateFileCommand command, CancellationToken cancellationToken) =>
        service.Validate(command.Request);
}





