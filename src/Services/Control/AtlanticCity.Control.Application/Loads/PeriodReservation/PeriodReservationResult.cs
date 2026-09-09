using AtlanticCity.Control.Domain.Loads;

namespace AtlanticCity.Control.Application.Loads.PeriodReservation;

public sealed record PeriodReservationResult(
    PeriodReservationDecision Decision,
    Guid? ConflictingLoadId = null,
    LoadStatus? ConflictingStatus = null)
{
    public bool IsSuccessful =>
        Decision is
            PeriodReservationDecision.Reserved or
            PeriodReservationDecision.AlreadyReserved;
}