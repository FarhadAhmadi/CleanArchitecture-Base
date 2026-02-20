using Application.Abstractions.Files;
using Application.Abstractions.Messaging;

namespace Application.Modules.Files;

public sealed record FilterFilesQuery(FilterFilesInput Request) : IQuery<IResult>;
internal sealed class FilterFilesQueryHandler(IFileUseCaseService service) : ResultWrappingQueryHandler<FilterFilesQuery>
{
    protected override async Task<IResult> HandleCore(FilterFilesQuery query, CancellationToken cancellationToken) =>
        await service.FilterAsync(query.Request, cancellationToken);
}





