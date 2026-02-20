using Application.Abstractions.Messaging;
using Application.Users.Auth;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class ResendVerificationCode : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/resend-verification-code", async (
            ResendVerificationCodeRequest request,
            ICommandHandler<ResendVerificationCodeCommand, IResult> handler,
            CancellationToken cancellationToken) =>
            (await handler.Handle(new ResendVerificationCodeCommand(request.Email), cancellationToken)).Match(static x => x, CustomResults.Problem))
        .WithTags(Tags.Users);
    }
}

