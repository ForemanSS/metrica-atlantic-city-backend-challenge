namespace AtlanticCity.Control.Application.Loads.PeriodReservation;

public interface ILoadPeriodReservationStore
{
    Task<PeriodReservationResult> TryReserveAsync(
        Guid loadId,
        string period,
        CancellationToken cancellationToken = default);
}