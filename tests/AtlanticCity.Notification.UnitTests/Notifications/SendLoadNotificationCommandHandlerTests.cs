using AtlanticCity.Notification.Application.Email;
using AtlanticCity.Notification.Application.Notifications;
using AtlanticCity.Notification.Application.Persistence;

namespace AtlanticCity.Notification.UnitTests.Notifications;

public sealed class SendLoadNotificationCommandHandlerTests
{
    [Fact]
    public async Task Handle_WhenLoadIsCompleted_ShouldSendEmailAndMarkNotified()
    {
        var loadId =
            Guid.NewGuid();

        var store =
            new FakeNotificationStore
            {
                Snapshot =
                    CreateSnapshot(
                        loadId,
                        "Completed",
                        "Success"),

                MarkResult = true
            };

        var emailSender =
            new FakeEmailSender();

        var handler =
            new SendLoadNotificationCommandHandler(
                store,
                emailSender);

        var command =
            CreateCommand(
                loadId,
                userEmail: "untrusted@message.test");

        var result =
            await handler.HandleAsync(
                command);

        Assert.True(
            result.Sent);

        Assert.False(
            result.AlreadyNotified);

        Assert.Equal(
            1,
            emailSender.SendCount);

        Assert.NotNull(
            emailSender.LastEmail);

        Assert.Equal(
            $"atlanticcity-load-{loadId:N}@atlanticcity.local",
            emailSender.LastEmail!.MessageId);

        Assert.Equal(
            "admin@atlanticcity.pe",
            emailSender.LastEmail!.To);

        Assert.Equal(
            "Atlantic City - Carga Exitosa",
            emailSender.LastEmail.Subject);

        Assert.Contains(
            "Resultado: Exitosa",
            emailSender.LastEmail.TextBody);

        Assert.Contains(
            "Total de registros: 10",
            emailSender.LastEmail.TextBody);

        Assert.Contains(
            "Insertados: 10",
            emailSender.LastEmail.TextBody);

        Assert.Equal(
            1,
            store.MarkNotifiedCount);

        Assert.Equal(
            command.CorrelationId,
            store.LastCorrelationId);
    }

    [Fact]
    public async Task Handle_WhenLoadIsAlreadyNotified_ShouldSkipEmail()
    {
        var loadId =
            Guid.NewGuid();

        var store =
            new FakeNotificationStore
            {
                Snapshot =
                    CreateSnapshot(
                        loadId,
                        "Notified",
                        "Success")
            };

        var emailSender =
            new FakeEmailSender();

        var handler =
            new SendLoadNotificationCommandHandler(
                store,
                emailSender);

        var result =
            await handler.HandleAsync(
                CreateCommand(loadId));

        Assert.False(
            result.Sent);

        Assert.True(
            result.AlreadyNotified);

        Assert.Equal(
            0,
            emailSender.SendCount);

        Assert.Equal(
            0,
            store.MarkNotifiedCount);
    }

    [Fact]
    public async Task Handle_WhenLoadDoesNotExist_ShouldThrow()
    {
        var store =
            new FakeNotificationStore
            {
                Snapshot = null
            };

        var emailSender =
            new FakeEmailSender();

        var handler =
            new SendLoadNotificationCommandHandler(
                store,
                emailSender);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () =>
                handler.HandleAsync(
                    CreateCommand(
                        Guid.NewGuid())));

        Assert.Equal(
            0,
            emailSender.SendCount);

        Assert.Equal(
            0,
            store.MarkNotifiedCount);
    }

    [Fact]
    public async Task Handle_WhenLoadIsNotCompleted_ShouldThrow()
    {
        var loadId =
            Guid.NewGuid();

        var store =
            new FakeNotificationStore
            {
                Snapshot =
                    CreateSnapshot(
                        loadId,
                        "Processing",
                        "Pending")
            };

        var emailSender =
            new FakeEmailSender();

        var handler =
            new SendLoadNotificationCommandHandler(
                store,
                emailSender);

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () =>
                    handler.HandleAsync(
                        CreateCommand(loadId)));

        Assert.Contains(
            "cannot be notified",
            exception.Message);

        Assert.Equal(
            0,
            emailSender.SendCount);

        Assert.Equal(
            0,
            store.MarkNotifiedCount);
    }

    [Fact]
    public async Task Handle_WhenEmailFails_ShouldNotMarkLoadAsNotified()
    {
        var loadId =
            Guid.NewGuid();

        var store =
            new FakeNotificationStore
            {
                Snapshot =
                    CreateSnapshot(
                        loadId,
                        "Completed",
                        "Success")
            };

        var emailSender =
            new FakeEmailSender
            {
                ExceptionToThrow =
                    new InvalidOperationException(
                        "SMTP unavailable.")
            };

        var handler =
            new SendLoadNotificationCommandHandler(
                store,
                emailSender);

        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () =>
                    handler.HandleAsync(
                        CreateCommand(loadId)));

        Assert.Equal(
            "SMTP unavailable.",
            exception.Message);

        Assert.Equal(
            1,
            emailSender.SendCount);

        Assert.Equal(
            0,
            store.MarkNotifiedCount);
    }

    private static NotificationLoadSnapshot CreateSnapshot(
        Guid loadId,
        string status,
        string result)
    {
        return new NotificationLoadSnapshot(
            loadId,
            status,
            result,
            "admin@atlanticcity.pe",
            "stored-correlation-id");
    }

    private static SendLoadNotificationCommand CreateCommand(
        Guid loadId,
        string userEmail = "admin@atlanticcity.pe")
    {
        return new SendLoadNotificationCommand(
            loadId,
            userEmail,
            "Success",
            10,
            10,
            0,
            0,
            Guid.NewGuid().ToString(),
            DateTimeOffset.UtcNow);
    }

    private sealed class FakeNotificationStore
        : INotificationStore
    {
        public NotificationLoadSnapshot? Snapshot { get; init; }

        public bool MarkResult { get; init; } =
            true;

        public int MarkNotifiedCount { get; private set; }

        public string? LastCorrelationId { get; private set; }

        public Task<NotificationLoadSnapshot?> FindAsync(
            Guid loadId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                Snapshot);
        }

        public Task<bool> MarkNotifiedAsync(
            Guid loadId,
            string correlationId,
            string message,
            CancellationToken cancellationToken = default)
        {
            MarkNotifiedCount++;

            LastCorrelationId =
                correlationId;

            return Task.FromResult(
                MarkResult);
        }
    }

    private sealed class FakeEmailSender
        : INotificationEmailSender
    {
        public int SendCount { get; private set; }

        public NotificationEmail? LastEmail { get; private set; }

        public Exception? ExceptionToThrow { get; init; }

        public Task SendAsync(
            NotificationEmail email,
            CancellationToken cancellationToken = default)
        {
            SendCount++;

            if (ExceptionToThrow is not null)
            {
                return Task.FromException(
                    ExceptionToThrow);
            }

            LastEmail =
                email;

            return Task.CompletedTask;
        }
    }
}