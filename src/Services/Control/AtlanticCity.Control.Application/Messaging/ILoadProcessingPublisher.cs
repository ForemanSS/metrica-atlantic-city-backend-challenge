namespace AtlanticCity.Control.Application.Messaging;

public interface ILoadProcessingPublisher
{
    Task PublishAsync(
        Guid loadId,
        string storagePath,
        string fileName,
        string userId,
        string userEmail,
        string correlationId,
        CancellationToken cancellationToken = default);
}