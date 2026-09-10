namespace AtlanticCity.MassiveLoad.Application.Persistence;

public sealed record LoadSnapshot(
    Guid LoadId,
    LoadProcessingState Status,
    LoadProcessingResult Result,
    string? Period,
    string UserEmail,
    string CorrelationId,
    int TotalRows = 0,
    int InsertedRows = 0,
    int ExistingRows = 0,
    int InvalidRows = 0);