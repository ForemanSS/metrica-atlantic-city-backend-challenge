using System.Net;
using AtlanticCity.Notification.Application.Email;
using AtlanticCity.Notification.Application.Persistence;

namespace AtlanticCity.Notification.Application.Notifications;

public sealed class SendLoadNotificationCommandHandler(
    INotificationStore store,
    INotificationEmailSender emailSender)
{
    public async Task<SendLoadNotificationResult> HandleAsync(
        SendLoadNotificationCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.LoadId == Guid.Empty)
        {
            throw new ArgumentException(
                "Load id cannot be empty.",
                nameof(command));
        }

        var load =
            await store.FindAsync(
                command.LoadId,
                cancellationToken)
            ?? throw new InvalidOperationException(
                $"Load '{command.LoadId}' was not found.");

        if (string.Equals(
                load.Status,
                "Notified",
                StringComparison.Ordinal))
        {
            return new SendLoadNotificationResult(
                command.LoadId,
                false,
                true);
        }

        if (!string.Equals(
                load.Status,
                "Completed",
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Load '{command.LoadId}' cannot be notified from status '{load.Status}'.");
        }

        var email =
            BuildEmail(
                command,
                load.UserEmail);

        await emailSender.SendAsync(
            email,
            cancellationToken);

        var marked =
            await store.MarkNotifiedAsync(
                command.LoadId,
                command.CorrelationId,
                $"Se envió la notificación por correo a {load.UserEmail}.",
                cancellationToken);

        return new SendLoadNotificationResult(
            command.LoadId,
            marked,
            !marked);
    }

    private static NotificationEmail BuildEmail(
        SendLoadNotificationCommand command,
        string destination)
    {
        var resultLabel =
            GetResultLabel(
                command.Result);

        var subject =
            $"Atlantic City - Carga {resultLabel}";

        var text =
            $"""
            La carga masiva {command.LoadId} ha finalizado.

            Resultado: {resultLabel}
            Total de registros: {command.TotalRows}
            Insertados: {command.InsertedRows}
            Existentes: {command.ExistingRows}
            Inválidos: {command.InvalidRows}
            Correlation ID: {command.CorrelationId}
            """;

        var encodedLoadId =
            WebUtility.HtmlEncode(
                command.LoadId.ToString());

        var encodedResult =
            WebUtility.HtmlEncode(
                resultLabel);

        var encodedCorrelationId =
            WebUtility.HtmlEncode(
                command.CorrelationId);

        var html =
            $"""
            <h2>Atlantic City - Carga Masiva</h2>

            <p>
                La carga <strong>{encodedLoadId}</strong>
                ha finalizado.
            </p>

            <table>
                <tr><td><strong>Resultado</strong></td><td>{encodedResult}</td></tr>
                <tr><td><strong>Total</strong></td><td>{command.TotalRows}</td></tr>
                <tr><td><strong>Insertados</strong></td><td>{command.InsertedRows}</td></tr>
                <tr><td><strong>Existentes</strong></td><td>{command.ExistingRows}</td></tr>
                <tr><td><strong>Inválidos</strong></td><td>{command.InvalidRows}</td></tr>
            </table>

            <p>
                Correlation ID:
                <code>{encodedCorrelationId}</code>
            </p>
            """;

        return new NotificationEmail(
            $"atlanticcity-load-{command.LoadId:N}@atlanticcity.local",
            destination,
            subject,
            text,
            html);
    }

    private static string GetResultLabel(
        string result)
    {
        return result switch
        {
            "Success" => "Exitosa",
            "Partial" => "Parcial",
            "Rejected" => "Rechazada",
            "Failed" => "Fallida",
            _ => result
        };
    }
}