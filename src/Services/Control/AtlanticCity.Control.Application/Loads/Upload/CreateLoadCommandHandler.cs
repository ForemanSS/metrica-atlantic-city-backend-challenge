using AtlanticCity.Control.Application.Loads.Persistence;
using AtlanticCity.Control.Application.Messaging;
using AtlanticCity.Control.Application.Storage;
using AtlanticCity.Control.Domain.Loads;

namespace AtlanticCity.Control.Application.Loads.Upload;

public sealed class CreateLoadCommandHandler(
    ILoadRepository loadRepository,
    ILoadFileStorage fileStorage,
    ILoadProcessingPublisher processingPublisher,
    LoadUploadSettings settings)
{
    public async Task<CreateLoadResult> HandleAsync(
        CreateLoadCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            command);

        var validationResult =
            Validate(command);

        if (validationResult is not null)
        {
            return validationResult;
        }

        var load =
            LoadFile.Create(
                command.FileName,
                command.UserId,
                command.UserEmail,
                command.CorrelationId);

        var pendingHistory =
            LoadStatusHistory.Create(
                load.Id,
                LoadStatus.Pending,
                LoadResult.Pending,
                command.CorrelationId,
                "Archivo recibido y pendiente de procesamiento.");

        loadRepository.Add(
            load);

        loadRepository.AddHistory(
            pendingHistory);

        await loadRepository.SaveChangesAsync(
            cancellationToken);

        try
        {
            var storagePath =
                await fileStorage.StoreAsync(
                    load.Id,
                    command.FileName,
                    command.Content,
                    command.ContentType,
                    cancellationToken);

            load.SetStoragePath(
                storagePath);

            await loadRepository.SaveChangesAsync(
                cancellationToken);

            await processingPublisher.PublishAsync(
                load.Id,
                storagePath,
                load.FileName,
                load.UserId,
                load.UserEmail,
                load.CorrelationId,
                cancellationToken);

            return CreateLoadResult.Accepted(
                load.Id);
        }
        catch
        {
            load.Fail(
                "No fue posible enviar la carga para su procesamiento asíncrono.");

            loadRepository.AddHistory(
                LoadStatusHistory.Create(
                    load.Id,
                    load.Status,
                    load.Result,
                    load.CorrelationId,
                    load.ErrorMessage));

            await loadRepository.SaveChangesAsync(
                cancellationToken);

            throw;
        }
    }

    private CreateLoadResult? Validate(
        CreateLoadCommand command)
    {
        if (string.IsNullOrWhiteSpace(
                command.FileName))
        {
            return CreateLoadResult.Invalid(
                "FILE_REQUIRED",
                "Debe seleccionar un archivo Excel.");
        }

        if (!string.Equals(
                Path.GetExtension(
                    command.FileName),
                ".xlsx",
                StringComparison.OrdinalIgnoreCase))
        {
            return CreateLoadResult.Invalid(
                "INVALID_FILE_EXTENSION",
                "Sólo se permiten archivos con extensión .xlsx.");
        }

        if (command.FileLength <= 0)
        {
            return CreateLoadResult.Invalid(
                "EMPTY_FILE",
                "El archivo cargado no puede estar vacío.");
        }

        if (command.FileLength >
            settings.MaxFileSizeBytes)
        {
            return CreateLoadResult.Invalid(
                "FILE_TOO_LARGE",
                $"El archivo supera el tamaño máximo permitido de {settings.MaxFileSizeBytes} bytes.");
        }

        if (command.Content is null ||
            !command.Content.CanRead)
        {
            return CreateLoadResult.Invalid(
                "INVALID_FILE_CONTENT",
                "No se puede leer el archivo cargado.");
        }

        if (string.IsNullOrWhiteSpace(
                command.UserId))
        {
            return CreateLoadResult.Invalid(
                "USER_REQUIRED",
                "Se requiere el identificador del usuario autenticado.");
        }

        if (string.IsNullOrWhiteSpace(
                command.UserEmail))
        {
            return CreateLoadResult.Invalid(
                "USER_EMAIL_REQUIRED",
                "Se requiere el correo del usuario autenticado.");
        }

        if (string.IsNullOrWhiteSpace(
                command.CorrelationId))
        {
            return CreateLoadResult.Invalid(
                "CORRELATION_ID_REQUIRED",
                "Se requiere un identificador de correlación.");
        }

        return null;
    }
}