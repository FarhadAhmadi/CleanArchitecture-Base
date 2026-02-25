using Application.Abstractions.Files;
using Application.Abstractions.Messaging;

namespace Application.Modules.Files;

public sealed record StreamFileQuery(Guid FileId, HttpContext HttpContext) : IQuery<IResult>;
internal sealed class StreamFileQueryHandler(IFileUseCaseService service) : ResultWrappingQueryHandler<StreamFileQuery>
{
    protected override async Task<IResult> HandleCore(StreamFileQuery query, CancellationToken cancellationToken) =>
        await service.StreamAsync(query.FileId, query.HttpContext, cancellationToken);
}





