namespace AtlanticCity.Notification.Application.Notifications;

public sealed record SendLoadNotificationCommand(
    Guid LoadId,
    string UserEmail,
    string Result,
    int TotalRows,
    int InsertedRows,
    int ExistingRows,
    int InvalidRows,
    string CorrelationId,
    DateTimeOffset CompletedAt);