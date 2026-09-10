namespace AtlanticCity.Notification.Infrastructure.Email;

internal sealed record SmtpOptions(
    string Host,
    int Port,
    string FromAddress,
    string FromName,
    string? Username,
    string? Password);