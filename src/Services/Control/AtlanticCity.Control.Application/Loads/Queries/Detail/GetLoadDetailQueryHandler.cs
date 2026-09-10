using AtlanticCity.Control.Application.Loads.Queries.Models;

namespace AtlanticCity.Control.Application.Loads.Queries.Detail;

public sealed class GetLoadDetailQueryHandler(
    ILoadQueryStore queryStore)
{
    public Task<LoadDetail?> HandleAsync(
        GetLoadDetailQuery query,
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

        return queryStore.FindByIdAsync(
            query.LoadId,
            cancellationToken);
    }
}