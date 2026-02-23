namespace Web.Api.Infrastructure;

internal static class SecurityConfigurationValidator
{
    private static readonly string[] WeakSecretMarkers =
    [
        "change-me",
        "super-duper-secret",
        "test-",
        "minioadmin"
    ];

    public static void Validate(IConfiguration configuration, IHostEnvironment environment)
    {
        if (environment.IsDevelopment() || environment.IsEnvironment("Testing"))
        {
            return;
        }

        List<string> errors = [];

        string jwtSecret = configuration["Jwt:Secret"]?.Trim() ?? string.Empty;
        if (jwtSecret.Length < 32 || LooksWeak(jwtSecret))
        {
            errors.Add("Jwt:Secret must be set to a strong value and must not use a placeholder.");
        }

        bool notificationsEnabled = bool.TryParse(configuration["Notifications:Enabled"], out bool notifEnabled) && notifEnabled;
        if (notificationsEnabled)
        {
            string notificationKey = configuration["Notifications:SensitiveDataEncryptionKey"]?.Trim() ?? string.Empty;
            if (notificationKey.Length < 32 || LooksWeak(notificationKey))
            {
                errors.Add("Notifications:SensitiveDataEncryptionKey must be at least 32 characters and must not use a placeholder.");
            }
        }

        bool fileStorageEnabled = bool.TryParse(configuration["FileStorage:Enabled"], out bool fileEnabled) && fileEnabled;
        if (fileStorageEnabled)
        {
            string accessKey = configuration["FileStorage:AccessKey"]?.Trim() ?? string.Empty;
            string secretKey = configuration["FileStorage:SecretKey"]?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(accessKey) || string.IsNullOrWhiteSpace(secretKey) || LooksWeak(secretKey))
            {
                errors.Add("FileStorage credentials must be provided through secure secret management and must not use defaults.");
            }
        }

        if (errors.Count != 0)
        {
            throw new InvalidOperationException("Security configuration validation failed: " + string.Join(" ", errors));
        }
    }

    private static bool LooksWeak(string value)
    {
        foreach (string marker in WeakSecretMarkers)
        {
            if (value.Contains(marker, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
