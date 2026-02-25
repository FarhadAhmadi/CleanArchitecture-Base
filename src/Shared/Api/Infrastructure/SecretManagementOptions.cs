namespace Web.Api.Infrastructure;

internal sealed class SecretManagementOptions
{
    public const string SectionName = "SecretManagement";

    public bool UseAzureKeyVault { get; init; }

    public string? KeyVaultUri { get; init; }
}
