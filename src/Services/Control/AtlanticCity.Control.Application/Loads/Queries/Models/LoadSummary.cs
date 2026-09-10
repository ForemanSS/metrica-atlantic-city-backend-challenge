namespace AtlanticCity.Control.Application.Loads.Queries.Models;

public sealed record LoadSummary(
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
    string UserEmail,
    string CorrelationId,
    DateTime CreatedAt,
    DateTime? CompletedAt,
    DateTime? NotifiedAt);