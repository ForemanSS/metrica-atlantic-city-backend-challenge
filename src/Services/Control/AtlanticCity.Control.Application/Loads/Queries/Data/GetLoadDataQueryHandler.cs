using AtlanticCity.Control.Application.Loads.Queries.Models;

namespace AtlanticCity.Control.Application.Loads.Queries.Data;

public sealed class GetLoadDataQueryHandler(
    ILoadQueryStore queryStore)
{
    private const int MaxPageSize = 100;

    public Task<PagedResult<ProcessedDataItem>?> HandleAsync(
        GetLoadDataQuery query,
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

        var page =
            Math.Max(
                query.Page,
                1);

        var pageSize =
            Math.Clamp(
                query.PageSize,
                1,
                MaxPageSize);

        return queryStore.GetDataAsync(
            query.LoadId,
            page,
            pageSize,
            cancellationToken);
    }
}