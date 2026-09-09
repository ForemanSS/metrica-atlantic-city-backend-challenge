namespace AtlanticCity.Control.Infrastructure.Persistence.PeriodReservation;

internal sealed class PeriodReservationDbResult
{
    public string ResultCode { get; init; } = null!;

    public Guid? ConflictingLoadId { get; init; }

    public string? ConflictingStatus { get; init; }
}