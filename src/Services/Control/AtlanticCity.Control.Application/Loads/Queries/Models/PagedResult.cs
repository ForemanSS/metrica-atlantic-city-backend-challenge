namespace AtlanticCity.Control.Application.Loads.Queries.Models;

public sealed record PagedResult<T>(
    IReadOnlyCollection<T> Items,
    int Page,
    int PageSize,
    int TotalItems)
{
    public int TotalPages =>
        TotalItems == 0
            ? 0
            : (int)Math.Ceiling(
                TotalItems / (double)PageSize);
}