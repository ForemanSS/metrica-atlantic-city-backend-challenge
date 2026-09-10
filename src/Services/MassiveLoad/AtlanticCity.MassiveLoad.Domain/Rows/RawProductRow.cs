namespace AtlanticCity.MassiveLoad.Domain.Rows;

public sealed record RawProductRow(
    int RowNumber,
    string? Period,
    string? ProductCode,
    string? ProductName,
    string? Description,
    string? Category,
    string? Quantity,
    string? Price)
{
    public bool IsCompletelyEmpty =>
        IsBlank(Period) &&
        IsBlank(ProductCode) &&
        IsBlank(ProductName) &&
        IsBlank(Description) &&
        IsBlank(Category) &&
        IsBlank(Quantity) &&
        IsBlank(Price);

    private static bool IsBlank(
        string? value)
    {
        return string.IsNullOrWhiteSpace(
            value);
    }
}