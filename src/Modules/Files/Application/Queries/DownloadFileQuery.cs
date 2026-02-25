using Application.Abstractions.Files;
using Application.Abstractions.Messaging;

namespace Application.Modules.Files;

public sealed record DownloadFileQuery(Guid FileId, HttpContext HttpContext) : IQuery<IResult>;
internal sealed class DownloadFileQueryHandler(IFileUseCaseService service) : ResultWrappingQueryHandler<DownloadFileQuery>
{
    protected override async Task<IResult> HandleCore(DownloadFileQuery query, CancellationToken cancellationToken) =>
        await service.DownloadAsync(query.FileId, query.HttpContext, cancellationToken);
}





