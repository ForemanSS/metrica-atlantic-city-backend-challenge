using System.Text.Json;
using AtlanticCity.MassiveLoad.Application.Persistence;
using AtlanticCity.MassiveLoad.Domain.Rows;
using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace AtlanticCity.MassiveLoad.Infrastructure.Persistence;

internal sealed class MassiveLoadStore(
    NpgsqlDataSource dataSource,
    ILogger<MassiveLoadStore> logger)
    : IMassiveLoadStore
{
    private const string FindLoadSql =
        """
        SELECT
            id AS LoadId,
            estado AS Status,
            resultado AS Result,
            periodo AS Period,
            usuario_email AS UserEmail,
            correlation_id AS CorrelationId,
            total_registros AS TotalRows,
            registros_insertados AS InsertedRows,
            registros_existentes AS ExistingRows,
            registros_invalidos AS InvalidRows
        FROM carga_archivo
        WHERE id = @LoadId;
        """;

    private const string ReservePeriodSql =
        """
        CALL sp_try_reserve_load_period(
            @LoadId,
            @Period,
            NULL,
            NULL,
            NULL
        );
        """;

    private const string InsertProcessingHistorySql =
        """
        INSERT INTO historial_estado_carga
        (
            id,
            carga_id,
            estado,
            resultado,
            mensaje,
            correlation_id,
            fecha_evento
        )
        SELECT
            @Id,
            @LoadId,
            'Processing',
            'Pending',
            'Se inició el procesamiento de la carga masiva.',
            @CorrelationId,
            CURRENT_TIMESTAMP
        WHERE NOT EXISTS
        (
            SELECT 1
            FROM historial_estado_carga
            WHERE carga_id = @LoadId
              AND estado = 'Processing'
              AND correlation_id = @CorrelationId
        );
        """;

    private const string InsertProductSql =
        """
        INSERT INTO data_procesada
        (
            id,
            carga_id,
            numero_fila,
            periodo,
            codigo_producto,
            nombre_producto,
            descripcion,
            categoria,
            cantidad,
            precio,
            fecha_registro
        )
        VALUES
        (
            @Id,
            @LoadId,
            @SourceRowNumber,
            @Period,
            @ProductCode,
            @ProductName,
            @Description,
            @Category,
            @Quantity,
            @Price,
            CURRENT_TIMESTAMP
        )
        ON CONFLICT DO NOTHING;
        """;

    private const string InsertErrorSql =
        """
        INSERT INTO detalle_carga_error
        (
            id,
            carga_id,
            numero_fila,
            codigo_error,
            campo,
            mensaje,
            datos_originales,
            fecha_registro
        )
        VALUES
        (
            @Id,
            @LoadId,
            @RowNumber,
            @ErrorCode,
            @Field,
            @Message,
            CAST(@RawData AS jsonb),
            CURRENT_TIMESTAMP
        );
        """;

    private const string MarkLoadedSql =
        """
        UPDATE carga_archivo
        SET
            estado = 'Loaded',
            total_registros = @TotalRows,
            registros_validos = @ValidRows,
            registros_insertados = @InsertedRows,
            registros_existentes = @ExistingRows,
            registros_invalidos = @InvalidRows,
            fecha_cargado = CURRENT_TIMESTAMP
        WHERE id = @LoadId
          AND estado = 'Processing';
        """;

    private const string InsertLoadedHistorySql =
        """
        INSERT INTO historial_estado_carga
        (
            id,
            carga_id,
            estado,
            resultado,
            mensaje,
            correlation_id,
            fecha_evento
        )
        VALUES
        (
            @Id,
            @LoadId,
            'Loaded',
            'Pending',
            @Message,
            @CorrelationId,
            CURRENT_TIMESTAMP
        );
        """;

    private const string MarkCompletedSql =
        """
        UPDATE carga_archivo
        SET
            estado = 'Completed',
            resultado = @Result,
            fecha_fin = CURRENT_TIMESTAMP,
            mensaje_error = NULL
        WHERE id = @LoadId
          AND estado = 'Loaded';
        """;

    private const string InsertCompletedHistorySql =
        """
        INSERT INTO historial_estado_carga
        (
            id,
            carga_id,
            estado,
            resultado,
            mensaje,
            correlation_id,
            fecha_evento
        )
        VALUES
        (
            @Id,
            @LoadId,
            'Completed',
            @Result,
            @Message,
            @CorrelationId,
            CURRENT_TIMESTAMP
        );
        """;

    public async Task<LoadSnapshot?> FindLoadAsync(
        Guid loadId,
        CancellationToken cancellationToken = default)
    {
        await using var connection =
            await dataSource.OpenConnectionAsync(
                cancellationToken);

        var db =
            await connection.QuerySingleOrDefaultAsync<LoadSnapshotDb>(
                new CommandDefinition(
                    FindLoadSql,
                    new
                    {
                        LoadId = loadId
                    },
                    cancellationToken:
                        cancellationToken));

        if (db is null)
        {
            return null;
        }

        return new LoadSnapshot(
            db.LoadId,
            ParseStatus(db.Status),
            ParseResult(db.Result),
            db.Period,
            db.UserEmail,
            db.CorrelationId,
            db.TotalRows,
            db.InsertedRows,
            db.ExistingRows,
            db.InvalidRows);
    }

    public async Task<PeriodReservationResult> TryReservePeriodAsync(
        Guid loadId,
        string period,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            period);

        await using var connection =
            await dataSource.OpenConnectionAsync(
                cancellationToken);

        var result =
            await connection.QuerySingleAsync<PeriodReservationDbResult>(
                new CommandDefinition(
                    ReservePeriodSql,
                    new
                    {
                        LoadId = loadId,
                        Period = period.Trim()
                    },
                    cancellationToken:
                        cancellationToken));

        var mapped =
            MapReservation(
                result);

        logger.LogInformation(
            "Period reservation evaluated. LoadId {LoadId}, Period {Period}, Decision {Decision}, Conflict {ConflictingLoadId}",
            loadId,
            period,
            mapped.Decision,
            mapped.ConflictingLoadId);

        return mapped;
    }

    public async Task AddProcessingHistoryAsync(
        Guid loadId,
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        await using var connection =
            await dataSource.OpenConnectionAsync(
                cancellationToken);

        await connection.ExecuteAsync(
            new CommandDefinition(
                InsertProcessingHistorySql,
                new
                {
                    Id = Guid.NewGuid(),
                    LoadId = loadId,
                    CorrelationId = correlationId
                },
                cancellationToken:
                    cancellationToken));
    }

    public async Task<LoadPersistenceResult> CompleteAsync(
        LoadPersistenceRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            request);

        await using var connection =
            await dataSource.OpenConnectionAsync(
                cancellationToken);

        await using var transaction =
            await connection.BeginTransactionAsync(
                cancellationToken);

        try
        {
            foreach (var error in request.ValidationErrors)
            {
                await InsertErrorAsync(
                    connection,
                    transaction,
                    request.LoadId,
                    error,
                    cancellationToken);
            }

            var insertedRows = 0;
            var existingRows = 0;

            foreach (var product in request.ValidProducts)
            {
                var affected =
                    await connection.ExecuteAsync(
                        new CommandDefinition(
                            InsertProductSql,
                            new
                            {
                                Id = Guid.NewGuid(),
                                LoadId = request.LoadId,
                                product.SourceRowNumber,
                                product.Period,
                                product.ProductCode,
                                product.ProductName,
                                product.Description,
                                product.Category,
                                product.Quantity,
                                product.Price
                            },
                            transaction,
                            cancellationToken:
                                cancellationToken));

                if (affected == 1)
                {
                    insertedRows++;
                    continue;
                }

                existingRows++;

                await InsertExistingProductErrorAsync(
                    connection,
                    transaction,
                    request.LoadId,
                    product,
                    cancellationToken);
            }

            var invalidRows =
                request.ValidationErrors
                    .Where(x => x.RowNumber.HasValue)
                    .Select(x => x.RowNumber!.Value)
                    .Distinct()
                    .Count();

            var validRows =
                request.ValidProducts.Count;

            if (request.TotalRows !=
                validRows + invalidRows)
            {
                throw new InvalidOperationException(
                    $"Load totals are inconsistent. Total={request.TotalRows}, Valid={validRows}, Invalid={invalidRows}.");
            }

            var result =
                invalidRows > 0 ||
                existingRows > 0
                    ? LoadProcessingResult.Partial
                    : LoadProcessingResult.Success;

            var loadedAffected =
                await connection.ExecuteAsync(
                    new CommandDefinition(
                        MarkLoadedSql,
                        new
                        {
                            request.LoadId,
                            request.TotalRows,
                            ValidRows = validRows,
                            InsertedRows = insertedRows,
                            ExistingRows = existingRows,
                            InvalidRows = invalidRows
                        },
                        transaction,
                        cancellationToken:
                            cancellationToken));

            if (loadedAffected != 1)
            {
                throw new InvalidOperationException(
                    $"Load '{request.LoadId}' could not transition from Processing to Loaded.");
            }

            await connection.ExecuteAsync(
                new CommandDefinition(
                    InsertLoadedHistorySql,
                    new
                    {
                        Id = Guid.NewGuid(),
                        request.LoadId,
                        request.CorrelationId,
                        Message =
                            $"Archivo Excel cargado. Total={request.TotalRows}, Válidos={validRows}, Insertados={insertedRows}, Existentes={existingRows}, Inválidos={invalidRows}."
                    },
                    transaction,
                    cancellationToken:
                        cancellationToken));

            var resultText =
                result.ToString();

            var completedAffected =
                await connection.ExecuteAsync(
                    new CommandDefinition(
                        MarkCompletedSql,
                        new
                        {
                            request.LoadId,
                            Result = resultText
                        },
                        transaction,
                        cancellationToken:
                            cancellationToken));

            if (completedAffected != 1)
            {
                throw new InvalidOperationException(
                    $"Load '{request.LoadId}' could not transition from Loaded to Completed.");
            }

            await connection.ExecuteAsync(
                new CommandDefinition(
                    InsertCompletedHistorySql,
                    new
                    {
                        Id = Guid.NewGuid(),
                        request.LoadId,
                        Result = resultText,
                        request.CorrelationId,
                        Message =
                            $"La carga masiva finalizó con resultado {GetResultLabel(result)}."
                    },
                    transaction,
                    cancellationToken:
                        cancellationToken));

            await transaction.CommitAsync(
                cancellationToken);

            logger.LogInformation(
                "Massive load persisted. LoadId {LoadId}, Total {Total}, Inserted {Inserted}, Existing {Existing}, Invalid {Invalid}, Result {Result}",
                request.LoadId,
                request.TotalRows,
                insertedRows,
                existingRows,
                invalidRows,
                result);

            return new LoadPersistenceResult(
                request.TotalRows,
                validRows,
                insertedRows,
                existingRows,
                invalidRows,
                result);
        }
        catch
        {
            await transaction.RollbackAsync(
                cancellationToken);

            throw;
        }
    }

    public Task RejectAsync(
        Guid loadId,
        string correlationId,
        string errorCode,
        string message,
        CancellationToken cancellationToken = default)
    {
        return FinishWithErrorAsync(
            loadId,
            correlationId,
            errorCode,
            message,
            LoadProcessingResult.Rejected,
            cancellationToken);
    }

    public Task FailAsync(
        Guid loadId,
        string correlationId,
        string errorCode,
        string message,
        CancellationToken cancellationToken = default)
    {
        return FinishWithErrorAsync(
            loadId,
            correlationId,
            errorCode,
            message,
            LoadProcessingResult.Failed,
            cancellationToken);
    }

    private async Task FinishWithErrorAsync(
        Guid loadId,
        string correlationId,
        string errorCode,
        string message,
        LoadProcessingResult result,
        CancellationToken cancellationToken)
    {
        const string updateSql =
            """
            UPDATE carga_archivo
            SET
                estado = 'Completed',
                resultado = @Result,
                mensaje_error = @Message,
                fecha_fin = CURRENT_TIMESTAMP
            WHERE id = @LoadId
              AND estado IN (
                  'Pending',
                  'Processing'
              );
            """;

        await using var connection =
            await dataSource.OpenConnectionAsync(
                cancellationToken);

        await using var transaction =
            await connection.BeginTransactionAsync(
                cancellationToken);

        try
        {
            var resultText =
                result.ToString();

            var affected =
                await connection.ExecuteAsync(
                    new CommandDefinition(
                        updateSql,
                        new
                        {
                            LoadId = loadId,
                            Result = resultText,
                            Message = message
                        },
                        transaction,
                        cancellationToken:
                            cancellationToken));

            if (affected == 0)
            {
                var currentStatus =
                    await connection.QuerySingleOrDefaultAsync<string?>(
                        new CommandDefinition(
                            """
                            SELECT estado
                            FROM carga_archivo
                            WHERE id = @LoadId;
                            """,
                            new
                            {
                                LoadId = loadId
                            },
                            transaction,
                            cancellationToken:
                                cancellationToken));

                if (currentStatus is
                    "Completed" or
                    "Notified")
                {
                    await transaction.RollbackAsync(
                        cancellationToken);

                    return;
                }

                throw new InvalidOperationException(
                    $"Load '{loadId}' could not be marked as {resultText}.");
            }

            await connection.ExecuteAsync(
                new CommandDefinition(
                    InsertErrorSql,
                    new
                    {
                        Id = Guid.NewGuid(),
                        LoadId = loadId,
                        RowNumber = (int?)null,
                        ErrorCode = errorCode,
                        Field = (string?)null,
                        Message = message,
                        RawData = (string?)null
                    },
                    transaction,
                    cancellationToken:
                        cancellationToken));

            await connection.ExecuteAsync(
                new CommandDefinition(
                    InsertCompletedHistorySql,
                    new
                    {
                        Id = Guid.NewGuid(),
                        LoadId = loadId,
                        Result = resultText,
                        CorrelationId = correlationId,
                        Message = message
                    },
                    transaction,
                    cancellationToken:
                        cancellationToken));

            await transaction.CommitAsync(
                cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(
                cancellationToken);

            throw;
        }
    }

    private static Task InsertErrorAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        Guid loadId,
        RowProcessingError error,
        CancellationToken cancellationToken)
    {
        var rawData =
            error.RawRow is null
                ? null
                : JsonSerializer.Serialize(
                    error.RawRow);

        return connection.ExecuteAsync(
            new CommandDefinition(
                InsertErrorSql,
                new
                {
                    Id = Guid.NewGuid(),
                    LoadId = loadId,
                    error.RowNumber,
                    error.ErrorCode,
                    error.Field,
                    error.Message,
                    RawData = rawData
                },
                transaction,
                cancellationToken:
                    cancellationToken));
    }

    private static Task InsertExistingProductErrorAsync(
        NpgsqlConnection connection,
        NpgsqlTransaction transaction,
        Guid loadId,
        ProcessedProduct product,
        CancellationToken cancellationToken)
    {
        var rawData =
            JsonSerializer.Serialize(
                product);

        return connection.ExecuteAsync(
            new CommandDefinition(
                InsertErrorSql,
                new
                {
                    Id = Guid.NewGuid(),
                    LoadId = loadId,
                    RowNumber =
                        (int?)product.SourceRowNumber,
                    ErrorCode =
                        "PRODUCT_EXISTS",
                    Field =
                        "CodigoProducto",
                    Message =
                        $"El código de producto '{product.ProductCode}' ya existe y no fue insertado.",
                    RawData = rawData
                },
                transaction,
                cancellationToken:
                    cancellationToken));
    }

    private static string GetResultLabel(
        LoadProcessingResult result)
    {
        return result switch
        {
            LoadProcessingResult.Success =>
                "exitoso",

            LoadProcessingResult.Partial =>
                "parcial",

            LoadProcessingResult.Rejected =>
                "rechazado",

            LoadProcessingResult.Failed =>
                "fallido",

            _ =>
                "pendiente"
        };
    }

    private static PeriodReservationResult MapReservation(
        PeriodReservationDbResult result)
    {
        LoadProcessingState? conflictingStatus =
            string.IsNullOrWhiteSpace(
                    result.ConflictingStatus)
                ? null
                : ParseStatus(
                    result.ConflictingStatus);

        return result.ResultCode switch
        {
            "RESERVED" =>
                new PeriodReservationResult(
                    PeriodReservationDecision.Reserved),

            "ALREADY_RESERVED" =>
                new PeriodReservationResult(
                    PeriodReservationDecision.AlreadyReserved),

            "BLOCKED" =>
                new PeriodReservationResult(
                    PeriodReservationDecision.Blocked,
                    result.ConflictingLoadId,
                    conflictingStatus),

            "REJECTED" =>
                new PeriodReservationResult(
                    PeriodReservationDecision.Rejected,
                    result.ConflictingLoadId,
                    conflictingStatus),

            "LOAD_NOT_FOUND" =>
                new PeriodReservationResult(
                    PeriodReservationDecision.LoadNotFound),

            "INVALID_STATUS" =>
                new PeriodReservationResult(
                    PeriodReservationDecision.InvalidStatus),

            _ =>
                throw new InvalidOperationException(
                    $"Unsupported reservation result '{result.ResultCode}'.")
        };
    }

    private static LoadProcessingState ParseStatus(
        string value)
    {
        return Enum.TryParse<LoadProcessingState>(
            value,
            false,
            out var status)
            ? status
            : throw new InvalidOperationException(
                $"Unsupported load status '{value}'.");
    }

    private static LoadProcessingResult ParseResult(
        string value)
    {
        return Enum.TryParse<LoadProcessingResult>(
            value,
            false,
            out var result)
            ? result
            : throw new InvalidOperationException(
                $"Unsupported load result '{value}'.");
    }
}