using Application.Abstractions.Files;
using Application.Abstractions.Messaging;

namespace Application.Modules.Files;

public sealed record GetFilePermissionsQuery(Guid FileId) : IQuery<IResult>;
internal sealed class GetFilePermissionsQueryHandler(IFileUseCaseService service) : ResultWrappingQueryHandler<GetFilePermissionsQuery>
{
    protected override async Task<IResult> HandleCore(GetFilePermissionsQuery query, CancellationToken cancellationToken) =>
        await service.GetPermissionsAsync(query.FileId, cancellationToken);
}





