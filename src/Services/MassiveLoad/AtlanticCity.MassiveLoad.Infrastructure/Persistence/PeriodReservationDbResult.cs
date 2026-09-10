namespace AtlanticCity.MassiveLoad.Infrastructure.Persistence;

internal sealed class PeriodReservationDbResult
{
    public string ResultCode { get; init; } = null!;

    public Guid? ConflictingLoadId { get; init; }

    public string? ConflictingStatus { get; init; }
}