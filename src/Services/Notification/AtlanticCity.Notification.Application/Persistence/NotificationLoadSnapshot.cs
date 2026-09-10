namespace AtlanticCity.Notification.Application.Persistence;

public sealed record NotificationLoadSnapshot(
    Guid LoadId,
    string Status,
    string Result,
    string UserEmail,
    string CorrelationId);