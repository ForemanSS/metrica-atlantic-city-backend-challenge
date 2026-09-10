namespace AtlanticCity.Control.Application.Loads.Queries.Models;

public sealed record LoadHistoryItem(
    Guid Id,
    string Status,
    string Result,
    string? Message,
    string CorrelationId,
    DateTime OccurredAt);