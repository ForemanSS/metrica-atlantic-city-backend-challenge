namespace AtlanticCity.Control.Application.Loads.Queries.Data;

public sealed record GetLoadDataQuery(
    Guid LoadId,
    int Page = 1,
    int PageSize = 50);