using AtlanticCity.MassiveLoad.Domain.Rows;

namespace AtlanticCity.MassiveLoad.Domain.Periods;

public static class PeriodResolver
{
    public static PeriodResolution Resolve(
        IEnumerable<RawProductRow> rows)
    {
        ArgumentNullException.ThrowIfNull(
            rows);

        var nonEmptyRows =
            rows
                .Where(
                    x => !x.IsCompletelyEmpty)
                .ToArray();

        if (nonEmptyRows.Length == 0)
        {
            return PeriodResolution.Failure(
                PeriodResolutionStatus.NoData,
                "EMPTY_FILE",
                "El archivo Excel no contiene filas con datos.");
        }

        var periods =
            nonEmptyRows
                .Select(
                    x => x.Period?.Trim())
                .Where(
                    x => !string.IsNullOrWhiteSpace(x))
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .ToArray();

        if (periods.Length == 0)
        {
            return PeriodResolution.Failure(
                PeriodResolutionStatus.MissingPeriod,
                "PERIOD_REQUIRED",
                "El archivo Excel debe contener un valor de Periodo.");
        }

        if (periods.Length > 1)
        {
            return PeriodResolution.Failure(
                PeriodResolutionStatus.MultiplePeriods,
                "MULTIPLE_PERIODS",
                "Todas las filas del archivo Excel deben pertenecer al mismo Periodo.");
        }

        var period =
            periods[0]!;

        if (period.Length > 20)
        {
            return PeriodResolution.Failure(
                PeriodResolutionStatus.InvalidPeriod,
                "INVALID_PERIOD",
                "Periodo no puede superar los 20 caracteres.");
        }

        return PeriodResolution.Success(
            period);
    }
}