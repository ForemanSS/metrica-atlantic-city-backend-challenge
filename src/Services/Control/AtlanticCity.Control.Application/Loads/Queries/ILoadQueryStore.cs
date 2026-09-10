using AtlanticCity.Control.Application.Loads.Queries.Models;

namespace AtlanticCity.Control.Application.Loads.Queries;

public interface ILoadQueryStore
{
    Task<PagedResult<LoadSummary>> ListAsync(
        int page,
        int pageSize,
        string? period,
        string? status,
        string? result,
        CancellationToken cancellationToken = default);

    Task<LoadDetail?> FindByIdAsync(
        Guid loadId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<LoadHistoryItem>?> GetHistoryAsync(
        Guid loadId,
        CancellationToken cancellationToken = default);

    Task<PagedResult<ProcessedDataItem>?> GetDataAsync(
        Guid loadId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<PagedResult<LoadErrorItem>?> GetErrorsAsync(
        Guid loadId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}