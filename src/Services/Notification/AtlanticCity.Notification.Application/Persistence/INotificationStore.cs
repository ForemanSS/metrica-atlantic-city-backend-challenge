namespace AtlanticCity.Notification.Application.Persistence;

public interface INotificationStore
{
    Task<NotificationLoadSnapshot?> FindAsync(
        Guid loadId,
        CancellationToken cancellationToken = default);

    Task<bool> MarkNotifiedAsync(
        Guid loadId,
        string correlationId,
        string message,
        CancellationToken cancellationToken = default);
}