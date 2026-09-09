namespace AtlanticCity.Control.Domain.Loads;

public sealed class LoadStatusHistory
{
    private LoadStatusHistory()
    {
    }

    public Guid Id { get; set; }

    public Guid LoadId { get; set; }

    public LoadStatus Status { get; set; }

    public LoadResult Result { get; set; }

    public string? Message { get; set; }

    public string CorrelationId { get; set; } = null!;

    public DateTimeOffset OccurredAt { get; set; }
}