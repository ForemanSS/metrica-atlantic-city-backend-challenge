using AtlanticCity.Control.Application.Loads.Queries.Models;

namespace AtlanticCity.Control.Application.Loads.Queries.List;

public sealed class ListLoadsQueryHandler(
    ILoadQueryStore queryStore)
{
    private const int MaxPageSize = 100;

    public Task<PagedResult<LoadSummary>> HandleAsync(
        ListLoadsQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            query);

        var page =
            Math.Max(
                query.Page,
                1);

        var pageSize =
            Math.Clamp(
                query.PageSize,
                1,
                MaxPageSize);

        return queryStore.ListAsync(
            page,
            pageSize,
            Normalize(query.Period),
            Normalize(query.Status),
            Normalize(query.Result),
            cancellationToken);
    }

    private static string? Normalize(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}