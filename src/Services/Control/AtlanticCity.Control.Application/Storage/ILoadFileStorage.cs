namespace AtlanticCity.Control.Application.Storage;

public interface ILoadFileStorage
{
    Task<string> StoreAsync(
        Guid loadId,
        string fileName,
        Stream content,
        string? contentType,
        CancellationToken cancellationToken = default);
}