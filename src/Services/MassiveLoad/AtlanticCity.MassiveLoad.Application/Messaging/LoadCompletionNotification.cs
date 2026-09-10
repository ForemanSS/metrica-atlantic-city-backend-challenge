using AtlanticCity.MassiveLoad.Application.Persistence;

namespace AtlanticCity.MassiveLoad.Application.Messaging;

public sealed record LoadCompletionNotification(
    Guid LoadId,
    string UserEmail,
    LoadProcessingResult Result,
    int TotalRows,
    int InsertedRows,
    int ExistingRows,
    int InvalidRows,
    string CorrelationId,
    DateTimeOffset CompletedAt);