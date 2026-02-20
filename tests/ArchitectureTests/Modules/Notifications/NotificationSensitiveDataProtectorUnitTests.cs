using Infrastructure.Notifications;
using Shouldly;

namespace ArchitectureTests.Modules.Notifications;

public sealed class NotificationSensitiveDataProtectorUnitTests
{
    [Fact]
    public void ProtectAndUnprotect_ShouldRoundTrip()
    {
        NotificationSensitiveDataProtector protector = CreateProtector("unit-test-key");

        string cipher = protector.Protect("user@test.local");
        string plain = protector.Unprotect(cipher);

        plain.ShouldBe("user@test.local");
    }

    [Fact]
    public void Unprotect_WithWrongKey_ShouldThrowCryptographicException()
    {
        NotificationSensitiveDataProtector writer = CreateProtector("writer-key");
        NotificationSensitiveDataProtector reader = CreateProtector("reader-key");

        string cipher = writer.Protect("user@test.local");

        Should.Throw<System.Security.Cryptography.CryptographicException>(() => reader.Unprotect(cipher));
    }

    [Fact]
    public void Unprotect_WithInvalidPayload_ShouldThrowCryptographicException()
    {
        NotificationSensitiveDataProtector protector = CreateProtector("unit-test-key");

        Should.Throw<System.Security.Cryptography.CryptographicException>(() => protector.Unprotect(Convert.ToBase64String([1, 2, 3])));
    }

    private static NotificationSensitiveDataProtector CreateProtector(string key)
    {
        return new NotificationSensitiveDataProtector(new NotificationOptions
        {
            SensitiveDataEncryptionKey = key
        });
    }
}
