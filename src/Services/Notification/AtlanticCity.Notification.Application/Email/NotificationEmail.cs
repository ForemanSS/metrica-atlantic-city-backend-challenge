namespace AtlanticCity.Notification.Application.Email;

public sealed record NotificationEmail(
    string MessageId,
    string To,
    string Subject,
    string TextBody,
    string HtmlBody);