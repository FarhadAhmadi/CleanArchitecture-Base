using Application.Abstractions.Messaging;
using Microsoft.AspNetCore.Authentication;

namespace Application.Users.Auth;

public sealed record StartOAuthQuery(string Provider, string? ReturnPath) : IQuery<IResult>;

internal sealed class StartOAuthQueryHandler : ResultWrappingQueryHandler<StartOAuthQuery>
{
    protected override Task<IResult> HandleCore(StartOAuthQuery query, CancellationToken cancellationToken)
    {
        _ = cancellationToken;
        return Task.FromResult(Start(query.Provider, query.ReturnPath));
    }

    private static IResult Start(string provider, string? returnUrl)
    {
        string? scheme = null;
        if (string.Equals(provider, "google", StringComparison.OrdinalIgnoreCase))
        {
            scheme = "Google";
        }
        else if (string.Equals(provider, "meta", StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(provider, "facebook", StringComparison.OrdinalIgnoreCase))
        {
            scheme = "Meta";
        }

        if (scheme is null)
        {
            return Results.BadRequest(new { error = "Unsupported provider." });
        }

        string callback = "/api/v1/users/oauth/callback";
        if (!string.IsNullOrWhiteSpace(returnUrl))
        {
            callback += $"?returnUrl={Uri.EscapeDataString(returnUrl)}";
        }

        AuthenticationProperties properties = new()
        {
            RedirectUri = callback
        };
        properties.Items["provider"] = scheme;

        return Results.Challenge(properties, [scheme]);
    }
}
