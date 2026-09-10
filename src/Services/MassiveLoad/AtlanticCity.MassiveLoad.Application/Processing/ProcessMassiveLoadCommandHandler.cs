using AtlanticCity.MassiveLoad.Application.Excel;
using AtlanticCity.MassiveLoad.Application.Persistence;
using AtlanticCity.MassiveLoad.Application.Storage;
using AtlanticCity.MassiveLoad.Domain.Periods;
using AtlanticCity.MassiveLoad.Domain.Rows;

namespace AtlanticCity.MassiveLoad.Application.Processing;

public sealed class ProcessMassiveLoadCommandHandler(
    ILoadFileSource fileSource,
    IExcelLoadReader excelReader,
    IMassiveLoadStore store)
{
    public async Task<ProcessMassiveLoadResult> HandleAsync(
        ProcessMassiveLoadCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            command);

        ValidateCommand(
            command);

        var snapshot =
            await store.FindLoadAsync(
                command.LoadId,
                cancellationToken);

        if (snapshot is null)
        {
            throw new InvalidOperationException(
                $"Load '{command.LoadId}' was not found.");
        }

        if (snapshot.Status is
            LoadProcessingState.Completed or
            LoadProcessingState.Notified)
        {
            return ProcessMassiveLoadResult.AlreadyFinalized(
                command.LoadId,
                snapshot.Status,
                snapshot.Result,
                snapshot.TotalRows,
                snapshot.InsertedRows,
                snapshot.ExistingRows,
                snapshot.InvalidRows);
        }

        if (snapshot.Status is not
            LoadProcessingState.Pending and not
            LoadProcessingState.Processing)
        {
            throw new InvalidOperationException(
                $"Load '{command.LoadId}' cannot be processed from status '{snapshot.Status}'.");
        }

        IReadOnlyCollection<RawProductRow> rows;

        try
        {
            await using var content =
                await fileSource.OpenReadAsync(
                    command.StoragePath,
                    cancellationToken);

            rows =
                await excelReader.ReadAsync(
                    content,
                    cancellationToken);
        }
        catch (ExcelLoadException exception)
        {
            await store.RejectAsync(
                command.LoadId,
                command.CorrelationId,
                exception.ErrorCode,
                exception.Message,
                cancellationToken);

            return ProcessMassiveLoadResult.Rejected(
                command.LoadId);
        }

        var periodResolution =
            PeriodResolver.Resolve(
                rows);

        if (!periodResolution.IsSuccess)
        {
            await store.RejectAsync(
                command.LoadId,
                command.CorrelationId,
                periodResolution.ErrorCode
                    ?? "INVALID_PERIOD",
                periodResolution.ErrorMessage
                    ?? "The Excel period could not be resolved.",
                cancellationToken);

            return ProcessMassiveLoadResult.Rejected(
                command.LoadId);
        }

        var period =
            periodResolution.Period!;

        var reservation =
            await store.TryReservePeriodAsync(
                command.LoadId,
                period,
                cancellationToken);

        var shouldContinue =
            await EvaluateReservationAsync(
                command,
                reservation,
                cancellationToken);

        if (!shouldContinue)
        {
            return ProcessMassiveLoadResult.Rejected(
                command.LoadId);
        }

        await store.AddProcessingHistoryAsync(
            command.LoadId,
            command.CorrelationId,
            cancellationToken);

        var validProducts =
            new List<ProcessedProduct>();

        var validationErrors =
            new List<RowProcessingError>();

        var totalRows =
            0;

        foreach (var row in rows)
        {
            cancellationToken
                .ThrowIfCancellationRequested();

            if (row.IsCompletelyEmpty)
            {
                continue;
            }

            totalRows++;

            var cleanResult =
                ProductRowCleaner.Clean(
                    command.LoadId,
                    row,
                    period);

            if (cleanResult.IsIgnored)
            {
                continue;
            }

            if (cleanResult.IsValid)
            {
                validProducts.Add(
                    cleanResult.Product!);

                continue;
            }

            validationErrors.AddRange(
                cleanResult.Errors);
        }

        var persistenceResult =
            await store.CompleteAsync(
                new LoadPersistenceRequest(
                    command.LoadId,
                    period,
                    totalRows,
                    validProducts,
                    validationErrors,
                    command.CorrelationId),
                cancellationToken);

        return ProcessMassiveLoadResult.Completed(
            command.LoadId,
            persistenceResult);
    }

    private async Task<bool> EvaluateReservationAsync(
        ProcessMassiveLoadCommand command,
        PeriodReservationResult reservation,
        CancellationToken cancellationToken)
    {
        switch (reservation.Decision)
        {
            case PeriodReservationDecision.Reserved:
            case PeriodReservationDecision.AlreadyReserved:
                return true;

            case PeriodReservationDecision.Blocked:
                {
                    var message =
                        reservation.ConflictingLoadId.HasValue
                            ? $"El período está siendo procesado actualmente por la carga '{reservation.ConflictingLoadId}'."
                            : "El período está siendo procesado actualmente por otra carga.";

                    await store.RejectAsync(
                        command.LoadId,
                        command.CorrelationId,
                        "PERIOD_IN_PROGRESS",
                        message,
                        cancellationToken);

                    return false;
                }

            case PeriodReservationDecision.Rejected:
                {
                    var message =
                        reservation.ConflictingLoadId.HasValue
                            ? $"El período ya fue procesado por la carga '{reservation.ConflictingLoadId}'."
                            : "El período ya fue procesado y no puede volver a cargarse.";

                    await store.RejectAsync(
                        command.LoadId,
                        command.CorrelationId,
                        "PERIOD_ALREADY_PROCESSED",
                        message,
                        cancellationToken);

                    return false;
                }

            case PeriodReservationDecision.LoadNotFound:
                throw new InvalidOperationException(
                    $"Load '{command.LoadId}' disappeared while reserving its period.");

            case PeriodReservationDecision.InvalidStatus:
                {
                    var current =
                        await store.FindLoadAsync(
                            command.LoadId,
                            cancellationToken);

                    if (current is not null &&
                        current.Status is
                            LoadProcessingState.Completed or
                            LoadProcessingState.Notified)
                    {
                        return false;
                    }

                    throw new InvalidOperationException(
                        $"Load '{command.LoadId}' has an invalid status for period reservation.");
                }

            default:
                throw new InvalidOperationException(
                    $"Unsupported reservation decision '{reservation.Decision}'.");
        }
    }

    private static void ValidateCommand(
        ProcessMassiveLoadCommand command)
    {
        if (command.LoadId == Guid.Empty)
        {
            throw new ArgumentException(
                "Load id cannot be empty.",
                nameof(command));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(
            command.StoragePath);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            command.FileName);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            command.UserId);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            command.UserEmail);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            command.CorrelationId);
    }
}