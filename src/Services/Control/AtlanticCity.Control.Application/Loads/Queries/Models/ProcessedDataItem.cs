namespace AtlanticCity.Control.Application.Loads.Queries.Models;

public sealed record ProcessedDataItem(
    Guid Id,
    int SourceRowNumber,
    string Period,
    string ProductCode,
    string ProductName,
    string Description,
    string Category,
    int Quantity,
    decimal Price,
    DateTime CreatedAt);