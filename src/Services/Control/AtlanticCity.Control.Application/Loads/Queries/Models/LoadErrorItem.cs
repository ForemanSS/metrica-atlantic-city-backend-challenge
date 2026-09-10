namespace AtlanticCity.Control.Application.Loads.Queries.Models;

public sealed record LoadErrorItem(
    Guid Id,
    int? RowNumber,
    string ErrorCode,
    string? Field,
    string Message,
    string? RawData,
    DateTime CreatedAt);