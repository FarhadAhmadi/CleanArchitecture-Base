using Application.Abstractions.Files;
using Application.Abstractions.Messaging;

namespace Application.Modules.Files;

public sealed record GetPublicFileByLinkQuery(string Token, HttpContext HttpContext) : IQuery<IResult>;
internal sealed class GetPublicFileByLinkQueryHandler(IFileUseCaseService service) : ResultWrappingQueryHandler<GetPublicFileByLinkQuery>
{
    protected override async Task<IResult> HandleCore(GetPublicFileByLinkQuery query, CancellationToken cancellationToken) =>
        await service.GetPublicByLinkAsync(query.Token, query.HttpContext, cancellationToken);
}





