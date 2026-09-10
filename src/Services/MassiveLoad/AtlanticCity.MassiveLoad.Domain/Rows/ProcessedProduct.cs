namespace AtlanticCity.MassiveLoad.Domain.Rows;

public sealed record ProcessedProduct(
    int SourceRowNumber,
    string Period,
    string ProductCode,
    string ProductName,
    string Description,
    string Category,
    int Quantity,
    decimal Price);