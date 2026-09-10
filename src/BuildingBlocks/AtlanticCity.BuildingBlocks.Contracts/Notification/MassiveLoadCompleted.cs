namespace AtlanticCity.BuildingBlocks.Contracts.Notification;

public sealed record MassiveLoadCompleted(
    Guid LoadId,
    string UserEmail,
    string Result,
    int TotalRows,
    int InsertedRows,
    int ExistingRows,
    int InvalidRows,
    string CorrelationId,
    DateTimeOffset CompletedAt);