namespace AtlanticCity.Control.Domain.Loads;

public sealed class LoadStatusHistory
{
    private LoadStatusHistory()
    {
    }

    private LoadStatusHistory(
        Guid id,
        Guid loadId,
        LoadStatus status,
        LoadResult result,
        string? message,
        string correlationId,
        DateTimeOffset occurredAt)
    {
        Id = id;
        LoadId = loadId;
        Status = status;
        Result = result;
        Message = message;
        CorrelationId = correlationId;
        OccurredAt = occurredAt;
    }

    public Guid Id { get; private set; }

    public Guid LoadId { get; private set; }

    public LoadStatus Status { get; private set; }

    public LoadResult Result { get; private set; }

    public string? Message { get; private set; }

    public string CorrelationId { get; private set; } = null!;

    public DateTimeOffset OccurredAt { get; private set; }

    public static LoadStatusHistory Create(
        Guid loadId,
        LoadStatus status,
        LoadResult result,
        string correlationId,
        string? message = null,
        DateTimeOffset? occurredAt = null)
    {
        if (loadId == Guid.Empty)
        {
            throw new ArgumentException(
                "Load id cannot be empty.",
                nameof(loadId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(
            correlationId);

        return new LoadStatusHistory(
            Guid.NewGuid(),
            loadId,
            status,
            result,
            string.IsNullOrWhiteSpace(message)
                ? null
                : message.Trim(),
            correlationId.Trim(),
            occurredAt ?? DateTimeOffset.UtcNow);
    }
}