namespace AtlanticCity.Control.Domain.Loads;

public sealed class LoadError
{
    private LoadError()
    {
    }

    public Guid Id { get; set; }

    public Guid LoadId { get; set; }

    public int? RowNumber { get; set; }

    public string ErrorCode { get; set; } = null!;

    public string? Field { get; set; }

    public string Message { get; set; } = null!;

    public string? RawData { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}