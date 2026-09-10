namespace AtlanticCity.MassiveLoad.Infrastructure.Persistence;

internal sealed class LoadSnapshotDb
{
    public Guid LoadId { get; init; }

    public string Status { get; init; } = null!;

    public string Result { get; init; } = null!;

    public string? Period { get; init; }

    public string UserEmail { get; init; } = null!;

    public string CorrelationId { get; init; } = null!;

    public int TotalRows { get; init; }

    public int InsertedRows { get; init; }

    public int ExistingRows { get; init; }

    public int InvalidRows { get; init; }
}
