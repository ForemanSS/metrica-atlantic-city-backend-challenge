using AtlanticCity.Control.Application.Loads.Queries;
using AtlanticCity.Control.Application.Loads.Queries.Models;
using Dapper;
using Npgsql;

namespace AtlanticCity.Control.Infrastructure.Persistence.Queries;

internal sealed class LoadQueryStore(
    NpgsqlDataSource dataSource)
    : ILoadQueryStore
{
    public async Task<PagedResult<LoadSummary>> ListAsync(
        int page,
        int pageSize,
        string? period,
        string? status,
        string? result,
        CancellationToken cancellationToken = default)
    {
        const string countSql =
            """
            SELECT COUNT(*)
            FROM carga_archivo
            WHERE (@Period IS NULL OR periodo = @Period)
              AND (@Status IS NULL OR estado = @Status)
              AND (@Result IS NULL OR resultado = @Result);
            """;

        const string dataSql =
            """
            SELECT
                id AS Id,
                nombre_archivo AS FileName,
                periodo AS Period,
                estado AS Status,
                resultado AS Result,
                total_registros AS TotalRows,
                registros_validos AS ValidRows,
                registros_insertados AS InsertedRows,
                registros_existentes AS ExistingRows,
                registros_invalidos AS InvalidRows,
                usuario_email AS UserEmail,
                correlation_id AS CorrelationId,
                fecha_registro AS CreatedAt,
                fecha_fin AS CompletedAt,
                fecha_notificacion AS NotifiedAt
            FROM carga_archivo
            WHERE (@Period IS NULL OR periodo = @Period)
              AND (@Status IS NULL OR estado = @Status)
              AND (@Result IS NULL OR resultado = @Result)
            ORDER BY fecha_registro DESC
            OFFSET @Offset
            LIMIT @PageSize;
            """;

        var parameters =
            new
            {
                Period = period,
                Status = status,
                Result = result,
                Offset =
                    (page - 1) * pageSize,
                PageSize = pageSize
            };

        await using var connection =
            await dataSource.OpenConnectionAsync(
                cancellationToken);

        var totalItems =
            await connection.ExecuteScalarAsync<int>(
                new CommandDefinition(
                    countSql,
                    parameters,
                    cancellationToken:
                        cancellationToken));

        var items =
            await connection.QueryAsync<LoadSummary>(
                new CommandDefinition(
                    dataSql,
                    parameters,
                    cancellationToken:
                        cancellationToken));

        return new PagedResult<LoadSummary>(
            items.AsList(),
            page,
            pageSize,
            totalItems);
    }

    public async Task<LoadDetail?> FindByIdAsync(
        Guid loadId,
        CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            SELECT
                id AS Id,
                nombre_archivo AS FileName,
                periodo AS Period,
                estado AS Status,
                resultado AS Result,
                total_registros AS TotalRows,
                registros_validos AS ValidRows,
                registros_insertados AS InsertedRows,
                registros_existentes AS ExistingRows,
                registros_invalidos AS InvalidRows,
                usuario_id AS UserId,
                usuario_email AS UserEmail,
                correlation_id AS CorrelationId,
                mensaje_error AS ErrorMessage,
                fecha_registro AS CreatedAt,
                fecha_inicio_proceso AS ProcessingStartedAt,
                fecha_cargado AS LoadedAt,
                fecha_fin AS CompletedAt,
                fecha_notificacion AS NotifiedAt
            FROM carga_archivo
            WHERE id = @LoadId;
            """;

        await using var connection =
            await dataSource.OpenConnectionAsync(
                cancellationToken);

        return await connection
            .QuerySingleOrDefaultAsync<LoadDetail>(
                new CommandDefinition(
                    sql,
                    new
                    {
                        LoadId = loadId
                    },
                    cancellationToken:
                        cancellationToken));
    }

    public async Task<IReadOnlyCollection<LoadHistoryItem>?> GetHistoryAsync(
        Guid loadId,
        CancellationToken cancellationToken = default)
    {
        const string sql =
            """
        SELECT EXISTS
        (
            SELECT 1
            FROM carga_archivo
            WHERE id = @LoadId
        );

        SELECT
            id AS Id,
            estado AS Status,
            resultado AS Result,
            mensaje AS Message,
            correlation_id AS CorrelationId,
            fecha_evento AS OccurredAt
        FROM historial_estado_carga
        WHERE carga_id = @LoadId
        ORDER BY fecha_evento ASC;
        """;

        await using var connection =
            await dataSource.OpenConnectionAsync(
                cancellationToken);

        using var grid =
            await connection.QueryMultipleAsync(
                new CommandDefinition(
                    sql,
                    new
                    {
                        LoadId = loadId
                    },
                    cancellationToken:
                        cancellationToken));

        var exists =
            await grid.ReadSingleAsync<bool>();

        if (!exists)
        {
            return null;
        }

        var items =
            await grid.ReadAsync<LoadHistoryItem>();

        return items.AsList();
    }

    public async Task<PagedResult<ProcessedDataItem>?> GetDataAsync(
        Guid loadId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        const string sql =
            """
        SELECT EXISTS
        (
            SELECT 1
            FROM carga_archivo
            WHERE id = @LoadId
        );

        SELECT COUNT(*)
        FROM data_procesada
        WHERE carga_id = @LoadId;

        SELECT
            id AS Id,
            numero_fila AS SourceRowNumber,
            periodo AS Period,
            codigo_producto AS ProductCode,
            nombre_producto AS ProductName,
            descripcion AS Description,
            categoria AS Category,
            cantidad AS Quantity,
            precio AS Price,
            fecha_registro AS CreatedAt
        FROM data_procesada
        WHERE carga_id = @LoadId
        ORDER BY numero_fila ASC
        OFFSET @Offset
        LIMIT @PageSize;
        """;

        var parameters =
            new
            {
                LoadId = loadId,
                Offset =
                    (page - 1) * pageSize,
                PageSize = pageSize
            };

        await using var connection =
            await dataSource.OpenConnectionAsync(
                cancellationToken);

        using var grid =
            await connection.QueryMultipleAsync(
                new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken:
                        cancellationToken));

        var exists =
            await grid.ReadSingleAsync<bool>();

        if (!exists)
        {
            return null;
        }

        var totalItems =
            await grid.ReadSingleAsync<int>();

        var items =
            await grid.ReadAsync<ProcessedDataItem>();

        return new PagedResult<ProcessedDataItem>(
            items.AsList(),
            page,
            pageSize,
            totalItems);
    }

    public async Task<PagedResult<LoadErrorItem>?> GetErrorsAsync(
        Guid loadId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        const string sql =
            """
        SELECT EXISTS
        (
            SELECT 1
            FROM carga_archivo
            WHERE id = @LoadId
        );

        SELECT COUNT(*)
        FROM detalle_carga_error
        WHERE carga_id = @LoadId;

        SELECT
            id AS Id,
            numero_fila AS RowNumber,
            codigo_error AS ErrorCode,
            campo AS Field,
            mensaje AS Message,
            datos_originales::text AS RawData,
            fecha_registro AS CreatedAt
        FROM detalle_carga_error
        WHERE carga_id = @LoadId
        ORDER BY
            numero_fila NULLS FIRST,
            fecha_registro ASC,
            codigo_error ASC
        OFFSET @Offset
        LIMIT @PageSize;
        """;

        var parameters =
            new
            {
                LoadId = loadId,
                Offset =
                    (page - 1) * pageSize,
                PageSize = pageSize
            };

        await using var connection =
            await dataSource.OpenConnectionAsync(
                cancellationToken);

        using var grid =
            await connection.QueryMultipleAsync(
                new CommandDefinition(
                    sql,
                    parameters,
                    cancellationToken:
                        cancellationToken));

        var exists =
            await grid.ReadSingleAsync<bool>();

        if (!exists)
        {
            return null;
        }

        var totalItems =
            await grid.ReadSingleAsync<int>();

        var items =
            await grid.ReadAsync<LoadErrorItem>();

        return new PagedResult<LoadErrorItem>(
            items.AsList(),
            page,
            pageSize,
            totalItems);
    }
}