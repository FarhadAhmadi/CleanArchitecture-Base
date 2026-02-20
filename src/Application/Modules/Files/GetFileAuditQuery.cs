using Application.Abstractions.Files;
using Application.Abstractions.Messaging;

namespace Application.Modules.Files;

public sealed record GetFileAuditQuery(Guid FileId) : IQuery<IResult>;
internal sealed class GetFileAuditQueryHandler(IFileUseCaseService service) : ResultWrappingQueryHandler<GetFileAuditQuery>
{
    protected override async Task<IResult> HandleCore(GetFileAuditQuery query, CancellationToken cancellationToken) =>
        await service.GetAuditAsync(query.FileId, cancellationToken);
}





