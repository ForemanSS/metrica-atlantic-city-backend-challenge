namespace AtlanticCity.Control.Application.Loads.Queries.Errors;

public sealed record GetLoadErrorsQuery(
    Guid LoadId,
    int Page = 1,
    int PageSize = 50);