namespace AtlanticCity.MassiveLoad.Application.Persistence;

public sealed record PeriodReservationResult(
    PeriodReservationDecision Decision,
    Guid? ConflictingLoadId = null,
    LoadProcessingState? ConflictingStatus = null);