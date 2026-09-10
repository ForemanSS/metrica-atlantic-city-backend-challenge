namespace AtlanticCity.Control.Application.Loads.Queries.List;

public sealed record ListLoadsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Period = null,
    string? Status = null,
    string? Result = null);