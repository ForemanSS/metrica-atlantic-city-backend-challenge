namespace AtlanticCity.MassiveLoad.Application.Storage;

public interface ILoadFileSource
{
    Task<Stream> OpenReadAsync(
        string storagePath,
        CancellationToken cancellationToken = default);
}