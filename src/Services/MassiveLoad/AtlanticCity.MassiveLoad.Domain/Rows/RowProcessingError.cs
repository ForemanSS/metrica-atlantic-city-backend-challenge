namespace AtlanticCity.MassiveLoad.Domain.Rows;

public sealed record RowProcessingError(
    int? RowNumber,
    string ErrorCode,
    string? Field,
    string Message,
    RawProductRow? RawRow = null);