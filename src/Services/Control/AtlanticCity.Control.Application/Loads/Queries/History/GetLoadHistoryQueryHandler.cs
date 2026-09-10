using AtlanticCity.Control.Application.Loads.Queries.Models;

namespace AtlanticCity.Control.Application.Loads.Queries.History;

public sealed class GetLoadHistoryQueryHandler(
    ILoadQueryStore queryStore)
{
    public Task<IReadOnlyCollection<LoadHistoryItem>?> HandleAsync(
        GetLoadHistoryQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            query);

        if (query.LoadId == Guid.Empty)
        {
            throw new ArgumentException(
                "LoadId cannot be empty.",
                nameof(query));
        }

        return queryStore.GetHistoryAsync(
            query.LoadId,
            cancellationToken);
    }
}