using System.Net.Http.Headers;
using AtlanticCity.Control.Application.Storage;
using Microsoft.Extensions.Logging;

namespace AtlanticCity.Control.Infrastructure.Storage;

internal sealed class SeaweedFileStorage(
    HttpClient httpClient,
    SeaweedFileStorageOptions options,
    ILogger<SeaweedFileStorage> logger)
    : ILoadFileStorage
{
    public async Task<string> StoreAsync(
        Guid loadId,
        string fileName,
        Stream content,
        string? contentType,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            fileName);

        ArgumentNullException.ThrowIfNull(
            content);

        var safeFileName =
            Path.GetFileName(
                fileName);

        var escapedFileName =
            Uri.EscapeDataString(
                safeFileName);

        var rootPath =
            options.RootPath
                .Trim('/');

        var relativePath =
            $"{rootPath}/{loadId:N}/{escapedFileName}";

        if (content.CanSeek)
        {
            content.Position = 0;
        }

        using var request =
            new HttpRequestMessage(
                HttpMethod.Put,
                relativePath);

        using var streamContent =
            new StreamContent(
                content);

        if (!string.IsNullOrWhiteSpace(
                contentType) &&
            MediaTypeHeaderValue.TryParse(
                contentType,
                out var mediaType))
        {
            streamContent.Headers.ContentType =
                mediaType;
        }
        else
        {
            streamContent.Headers.ContentType =
                new MediaTypeHeaderValue(
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }

        request.Content =
            streamContent;

        using var response =
            await httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

        response.EnsureSuccessStatusCode();

        var storagePath =
            $"seaweed://filer/{relativePath}";

        logger.LogInformation(
            "File stored in SeaweedFS. LoadId {LoadId}, StoragePath {StoragePath}",
            loadId,
            storagePath);

        return storagePath;
    }
}