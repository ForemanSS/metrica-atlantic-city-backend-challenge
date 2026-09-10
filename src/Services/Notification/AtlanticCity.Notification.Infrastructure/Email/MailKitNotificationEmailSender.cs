using AtlanticCity.Notification.Application.Email;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace AtlanticCity.Notification.Infrastructure.Email;

internal sealed class MailKitNotificationEmailSender(
    SmtpOptions options,
    ILogger<MailKitNotificationEmailSender> logger)
    : INotificationEmailSender
{
    public async Task SendAsync(
        NotificationEmail email,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(email);

        var message =
            new MimeMessage();

        message.MessageId =
            email.MessageId;

        message.From.Add(
            new MailboxAddress(
                options.FromName,
                options.FromAddress));

        message.To.Add(
            MailboxAddress.Parse(
                email.To));

        message.Subject =
            email.Subject;

        message.Body =
            new BodyBuilder
            {
                TextBody = email.TextBody,
                HtmlBody = email.HtmlBody
            }.ToMessageBody();

        using var client =
            new SmtpClient();

        await client.ConnectAsync(
            options.Host,
            options.Port,
            SecureSocketOptions.None,
            cancellationToken);

        var username =
            options.Username;

        var password =
            options.Password;

        if (!string.IsNullOrWhiteSpace(
                username))
        {
            if (string.IsNullOrWhiteSpace(
                    password))
            {
                throw new InvalidOperationException(
                    "SMTP password must be configured when SMTP username is configured.");
            }

            await client.AuthenticateAsync(
                username,
                password,
                cancellationToken);
        }

        await client.SendAsync(
            message,
            cancellationToken);

        await client.DisconnectAsync(
            true,
            cancellationToken);

        logger.LogInformation(
            "Notification email sent. To {Recipient}, Subject {Subject}",
            email.To,
            email.Subject);
    }
}