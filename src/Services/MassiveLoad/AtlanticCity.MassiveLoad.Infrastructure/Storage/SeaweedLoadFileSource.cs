using AtlanticCity.MassiveLoad.Application.Storage;
using Microsoft.Extensions.Logging;

namespace AtlanticCity.MassiveLoad.Infrastructure.Storage;

internal sealed class SeaweedLoadFileSource(
    HttpClient httpClient,
    ILogger<SeaweedLoadFileSource> logger)
    : ILoadFileSource
{
    public async Task<Stream> OpenReadAsync(
        string storagePath,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            storagePath);

        var relativePath =
            ResolveRelativePath(
                storagePath);

        logger.LogInformation(
            "Downloading massive load file from SeaweedFS. StoragePath {StoragePath}",
            storagePath);

        using var response =
            await httpClient.GetAsync(
                relativePath,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

        response.EnsureSuccessStatusCode();

        await using var source =
            await response.Content.ReadAsStreamAsync(
                cancellationToken);

        var destination =
            new MemoryStream();

        await source.CopyToAsync(
            destination,
            cancellationToken);

        destination.Position = 0;

        logger.LogInformation(
            "Massive load file downloaded from SeaweedFS. StoragePath {StoragePath}, Length {Length}",
            storagePath,
            destination.Length);

        return destination;
    }

    private static string ResolveRelativePath(
        string storagePath)
    {
        if (!Uri.TryCreate(
                storagePath,
                UriKind.Absolute,
                out var uri))
        {
            throw new InvalidOperationException(
                $"Invalid storage path '{storagePath}'.");
        }

        if (!string.Equals(
                uri.Scheme,
                "seaweed",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Unsupported storage scheme '{uri.Scheme}'.");
        }

        if (!string.Equals(
                uri.Host,
                "filer",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Unsupported SeaweedFS authority '{uri.Host}'.");
        }

        if (string.IsNullOrWhiteSpace(
                uri.AbsolutePath) ||
            uri.AbsolutePath == "/")
        {
            throw new InvalidOperationException(
                "SeaweedFS storage path does not contain a file path.");
        }

        return uri.AbsolutePath.TrimStart('/');
    }
}