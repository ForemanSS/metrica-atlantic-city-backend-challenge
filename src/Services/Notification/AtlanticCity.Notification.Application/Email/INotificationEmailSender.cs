namespace AtlanticCity.Notification.Application.Email;

public interface INotificationEmailSender
{
    Task SendAsync(
        NotificationEmail email,
        CancellationToken cancellationToken = default);
}