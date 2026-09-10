using AtlanticCity.Notification.Application.Persistence;
using Dapper;
using Npgsql;

namespace AtlanticCity.Notification.Infrastructure.Persistence;

internal sealed class NotificationStore(
    NpgsqlDataSource dataSource)
    : INotificationStore
{
    public async Task<NotificationLoadSnapshot?> FindAsync(
        Guid loadId,
        CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            SELECT
                id AS LoadId,
                estado AS Status,
                resultado AS Result,
                usuario_email AS UserEmail,
                correlation_id AS CorrelationId
            FROM carga_archivo
            WHERE id = @LoadId;
            """;

        await using var connection =
            await dataSource.OpenConnectionAsync(
                cancellationToken);

        return await connection
            .QuerySingleOrDefaultAsync<NotificationLoadSnapshot>(
                new CommandDefinition(
                    sql,
                    new
                    {
                        LoadId = loadId
                    },
                    cancellationToken:
                        cancellationToken));
    }

    public async Task<bool> MarkNotifiedAsync(
        Guid loadId,
        string correlationId,
        string message,
        CancellationToken cancellationToken = default)
    {
        await using var connection =
            await dataSource.OpenConnectionAsync(
                cancellationToken);

        await using var transaction =
            await connection.BeginTransactionAsync(
                cancellationToken);

        try
        {
            const string updateSql =
                """
                UPDATE carga_archivo
                SET
                    estado = 'Notified',
                    fecha_notificacion = CURRENT_TIMESTAMP
                WHERE id = @LoadId
                  AND estado = 'Completed';
                """;

            var affected =
                await connection.ExecuteAsync(
                    new CommandDefinition(
                        updateSql,
                        new
                        {
                            LoadId = loadId
                        },
                        transaction,
                        cancellationToken:
                            cancellationToken));

            if (affected == 0)
            {
                var status =
                    await connection
                        .QuerySingleOrDefaultAsync<string?>(
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

                if (status == "Notified")
                {
                    await transaction.RollbackAsync(
                        cancellationToken);

                    return false;
                }

                throw new InvalidOperationException(
                    $"Load '{loadId}' could not transition to Notified.");
            }

            const string historySql =
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
                    id,
                    'Notified',
                    resultado,
                    @Message,
                    @CorrelationId,
                    CURRENT_TIMESTAMP
                FROM carga_archivo
                WHERE id = @LoadId;
                """;

            await connection.ExecuteAsync(
                new CommandDefinition(
                    historySql,
                    new
                    {
                        Id = Guid.NewGuid(),
                        LoadId = loadId,
                        Message = message,
                        CorrelationId = correlationId
                    },
                    transaction,
                    cancellationToken:
                        cancellationToken));

            await transaction.CommitAsync(
                cancellationToken);

            return true;
        }
        catch
        {
            await transaction.RollbackAsync(
                cancellationToken);

            throw;
        }
    }
}