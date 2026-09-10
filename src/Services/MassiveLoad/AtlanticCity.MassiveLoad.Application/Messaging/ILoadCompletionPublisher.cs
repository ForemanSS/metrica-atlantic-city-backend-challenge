namespace AtlanticCity.MassiveLoad.Application.Messaging;

public interface ILoadCompletionPublisher
{
    Task PublishAsync(
        LoadCompletionNotification notification,
        CancellationToken cancellationToken = default);
}