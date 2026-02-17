namespace Web.Api.Infrastructure;

internal sealed class ExternalOAuthOptions
{
    public const string SectionName = "ExternalOAuth";

    public GoogleOAuthOptions Google { get; init; } = new();
    public MetaOAuthOptions Meta { get; init; } = new();

    internal sealed class GoogleOAuthOptions
    {
        public bool Enabled { get; init; }
        public string ClientId { get; init; } = string.Empty;
        public string ClientSecret { get; init; } = string.Empty;
    }

    internal sealed class MetaOAuthOptions
    {
        public bool Enabled { get; init; }
        public string AppId { get; init; } = string.Empty;
        public string AppSecret { get; init; } = string.Empty;
    }
}
