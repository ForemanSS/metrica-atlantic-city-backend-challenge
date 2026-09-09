using AtlanticCity.Control.Application.Loads.PeriodReservation;
using AtlanticCity.Control.Domain.Loads;
using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace AtlanticCity.Control.Infrastructure.Persistence.PeriodReservation;

internal sealed class LoadPeriodReservationStore(
    NpgsqlDataSource dataSource,
    ILogger<LoadPeriodReservationStore> logger)
    : ILoadPeriodReservationStore
{
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

    public async Task<PeriodReservationResult> TryReserveAsync(
        Guid loadId,
        string period,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(period);

        await using var connection =
            await dataSource.OpenConnectionAsync(cancellationToken);

        var command = new CommandDefinition(
            ReservePeriodSql,
            new
            {
                LoadId = loadId,
                Period = period.Trim()
            },
            cancellationToken: cancellationToken);

        var result =
            await connection.QuerySingleAsync<PeriodReservationDbResult>(
                command);

        var mappedResult = Map(result);

        logger.LogInformation(
            "Period reservation evaluated for LoadId {LoadId}, Period {Period}, Decision {Decision}, ConflictingLoadId {ConflictingLoadId}",
            loadId,
            period,
            mappedResult.Decision,
            mappedResult.ConflictingLoadId);

        return mappedResult;
    }

    private static PeriodReservationResult Map(
        PeriodReservationDbResult result)
    {
        var conflictingStatus =
            ParseStatus(result.ConflictingStatus);

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

            _ => throw new InvalidOperationException(
                $"Unsupported period reservation result '{result.ResultCode}'.")
        };
    }

    private static LoadStatus? ParseStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return null;
        }

        return Enum.TryParse<LoadStatus>(
            status,
            ignoreCase: false,
            out var parsed)
            ? parsed
            : throw new InvalidOperationException(
                $"Unsupported load status '{status}'.");
    }
}