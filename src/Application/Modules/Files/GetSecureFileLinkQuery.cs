using Application.Abstractions.Files;
using Application.Abstractions.Messaging;

namespace Application.Modules.Files;

public sealed record GetSecureFileLinkQuery(Guid FileId, string? Mode, HttpContext HttpContext) : IQuery<IResult>;
internal sealed class GetSecureFileLinkQueryHandler(IFileUseCaseService service) : ResultWrappingQueryHandler<GetSecureFileLinkQuery>
{
    protected override async Task<IResult> HandleCore(GetSecureFileLinkQuery query, CancellationToken cancellationToken) =>
        await service.GetSecureLinkAsync(query.FileId, query.Mode, query.HttpContext, cancellationToken);
}





