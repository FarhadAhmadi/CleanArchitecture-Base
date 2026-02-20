using Application.Abstractions.Notifications;

namespace Infrastructure.Notifications;

internal sealed class NotificationRecipientProtector(NotificationSensitiveDataProtector protector)
    : INotificationRecipientProtector
{
    public string Protect(string plainText) => protector.Protect(plainText);

    public string ComputeDeterministicHash(string value) =>
        NotificationSensitiveDataProtector.ComputeDeterministicHash(value);
}
