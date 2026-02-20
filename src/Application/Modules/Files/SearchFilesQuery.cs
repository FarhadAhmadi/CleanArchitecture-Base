using Application.Abstractions.Files;
using Application.Abstractions.Messaging;

namespace Application.Modules.Files;

public sealed record SearchFilesQuery(SearchFilesInput Request) : IQuery<IResult>;
internal sealed class SearchFilesQueryHandler(IFileUseCaseService service) : ResultWrappingQueryHandler<SearchFilesQuery>
{
    protected override async Task<IResult> HandleCore(SearchFilesQuery query, CancellationToken cancellationToken) =>
        await service.SearchAsync(query.Request, cancellationToken);
}





