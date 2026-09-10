namespace AtlanticCity.MassiveLoad.Application.Persistence;

public interface IMassiveLoadStore
{
    Task<LoadSnapshot?> FindLoadAsync(
        Guid loadId,
        CancellationToken cancellationToken = default);

    Task<PeriodReservationResult> TryReservePeriodAsync(
        Guid loadId,
        string period,
        CancellationToken cancellationToken = default);

    Task AddProcessingHistoryAsync(
        Guid loadId,
        string correlationId,
        CancellationToken cancellationToken = default);

    Task<LoadPersistenceResult> CompleteAsync(
        LoadPersistenceRequest request,
        CancellationToken cancellationToken = default);

    Task RejectAsync(
        Guid loadId,
        string correlationId,
        string errorCode,
        string message,
        CancellationToken cancellationToken = default);

    Task FailAsync(
        Guid loadId,
        string correlationId,
        string errorCode,
        string message,
        CancellationToken cancellationToken = default);
}