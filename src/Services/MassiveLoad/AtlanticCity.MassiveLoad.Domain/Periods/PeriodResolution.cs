namespace AtlanticCity.MassiveLoad.Domain.Periods;

public enum PeriodResolutionStatus
{
    Success = 1,
    NoData = 2,
    MissingPeriod = 3,
    MultiplePeriods = 4,
    InvalidPeriod = 5
}

public sealed record PeriodResolution(
    PeriodResolutionStatus Status,
    string? Period,
    string? ErrorCode,
    string? ErrorMessage)
{
    public bool IsSuccess =>
        Status ==
        PeriodResolutionStatus.Success;

    public static PeriodResolution Success(
        string period)
    {
        return new PeriodResolution(
            PeriodResolutionStatus.Success,
            period,
            null,
            null);
    }

    public static PeriodResolution Failure(
        PeriodResolutionStatus status,
        string errorCode,
        string errorMessage)
    {
        return new PeriodResolution(
            status,
            null,
            errorCode,
            errorMessage);
    }
}