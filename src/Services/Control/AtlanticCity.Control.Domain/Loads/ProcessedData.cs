namespace AtlanticCity.Control.Domain.Loads;

public sealed class ProcessedData
{
    private ProcessedData()
    {
    }

    public Guid Id { get; set; }

    public Guid LoadId { get; set; }

    public int SourceRowNumber { get; set; }

    public string Period { get; set; } = null!;

    public string ProductCode { get; set; } = null!;

    public string ProductName { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string Category { get; set; } = null!;

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}