namespace Application.Abstractions.Notifications;

public interface INotificationRecipientProtector
{
    string Protect(string plainText);

    string ComputeDeterministicHash(string value);
}
