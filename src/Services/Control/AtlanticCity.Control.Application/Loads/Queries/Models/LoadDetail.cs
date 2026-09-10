namespace AtlanticCity.Control.Application.Loads.Queries.Models;

public sealed record LoadDetail(
    Guid Id,
    string FileName,
    string? Period,
    string Status,
    string Result,
    int TotalRows,
    int ValidRows,
    int InsertedRows,
    int ExistingRows,
    int InvalidRows,
    string UserId,
    string UserEmail,
    string CorrelationId,
    string? ErrorMessage,
    DateTime CreatedAt,
    DateTime? ProcessingStartedAt,
    DateTime? LoadedAt,
    DateTime? CompletedAt,
    DateTime? NotifiedAt);