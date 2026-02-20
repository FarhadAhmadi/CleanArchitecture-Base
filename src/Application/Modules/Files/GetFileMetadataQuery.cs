using Application.Abstractions.Files;
using Application.Abstractions.Messaging;

namespace Application.Modules.Files;

public sealed record GetFileMetadataQuery(Guid FileId) : IQuery<IResult>;
internal sealed class GetFileMetadataQueryHandler(IFileUseCaseService service) : ResultWrappingQueryHandler<GetFileMetadataQuery>
{
    protected override async Task<IResult> HandleCore(GetFileMetadataQuery query, CancellationToken cancellationToken) =>
        await service.GetMetadataAsync(query.FileId, cancellationToken);
}





