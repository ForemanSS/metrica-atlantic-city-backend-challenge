namespace AtlanticCity.MassiveLoad.Application.Persistence;

public sealed record LoadPersistenceResult(
    int TotalRows,
    int ValidRows,
    int InsertedRows,
    int ExistingRows,
    int InvalidRows,
    LoadProcessingResult Result);