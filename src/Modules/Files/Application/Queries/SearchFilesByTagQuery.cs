using Application.Abstractions.Files;
using Application.Abstractions.Messaging;

namespace Application.Modules.Files;

public sealed record SearchFilesByTagQuery(string Tag) : IQuery<IResult>;
internal sealed class SearchFilesByTagQueryHandler(IFileUseCaseService service) : ResultWrappingQueryHandler<SearchFilesByTagQuery>
{
    protected override async Task<IResult> HandleCore(SearchFilesByTagQuery query, CancellationToken cancellationToken) =>
        await service.SearchByTagAsync(query.Tag, cancellationToken);
}





