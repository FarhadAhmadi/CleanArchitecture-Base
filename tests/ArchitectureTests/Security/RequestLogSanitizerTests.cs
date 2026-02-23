using Microsoft.AspNetCore.Http;
using Shouldly;
using Web.Api.Infrastructure;

namespace ArchitectureTests.Security;

public sealed class RequestLogSanitizerTests
{
    [Fact]
    public void SanitizeQueryString_ShouldRedactSensitiveKeys()
    {
        DefaultHttpContext context = new();
        context.Request.QueryString = new QueryString("?token=abc123&password=Secret!1&page=2");

        string sanitized = RequestLogSanitizer.SanitizeQueryString(context.Request);

        sanitized.ShouldContain("page=2");
        sanitized.ShouldContain("token=%5BREDACTED%5D");
        sanitized.ShouldContain("password=%5BREDACTED%5D");
        sanitized.ShouldNotContain("abc123");
        sanitized.ShouldNotContain("Secret!1");
    }
}
