using AtlanticCity.MassiveLoad.Domain.Rows;

namespace AtlanticCity.MassiveLoad.Application.Persistence;

public sealed record LoadPersistenceRequest(
    Guid LoadId,
    string Period,
    int TotalRows,
    IReadOnlyCollection<ProcessedProduct> ValidProducts,
    IReadOnlyCollection<RowProcessingError> ValidationErrors,
    string CorrelationId);