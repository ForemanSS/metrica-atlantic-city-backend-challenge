using AtlanticCity.MassiveLoad.Application.Persistence;

namespace AtlanticCity.MassiveLoad.Application.Processing;

public sealed record ProcessMassiveLoadResult(
    Guid LoadId,
    LoadProcessingState Status,
    LoadProcessingResult Result,
    int TotalRows,
    int InsertedRows,
    int ExistingRows,
    int InvalidRows,
    bool Skipped = false)
{
    public static ProcessMassiveLoadResult Completed(
        Guid loadId,
        LoadPersistenceResult persistenceResult)
    {
        return new ProcessMassiveLoadResult(
            loadId,
            LoadProcessingState.Completed,
            persistenceResult.Result,
            persistenceResult.TotalRows,
            persistenceResult.InsertedRows,
            persistenceResult.ExistingRows,
            persistenceResult.InvalidRows);
    }

    public static ProcessMassiveLoadResult Rejected(
        Guid loadId)
    {
        return new ProcessMassiveLoadResult(
            loadId,
            LoadProcessingState.Completed,
            LoadProcessingResult.Rejected,
            0,
            0,
            0,
            0);
    }

    public static ProcessMassiveLoadResult AlreadyFinalized(
        Guid loadId,
        LoadProcessingState status,
        LoadProcessingResult result,
        int totalRows,
        int insertedRows,
        int existingRows,
        int invalidRows)
    {
        return new ProcessMassiveLoadResult(
            loadId,
            status,
            result,
            totalRows,
            insertedRows,
            existingRows,
            invalidRows,
            true);
    }
}