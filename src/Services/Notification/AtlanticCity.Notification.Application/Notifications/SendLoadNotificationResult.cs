namespace AtlanticCity.Notification.Application.Notifications;

public sealed record SendLoadNotificationResult(
    Guid LoadId,
    bool Sent,
    bool AlreadyNotified);