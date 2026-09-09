namespace AtlanticCity.Control.Domain.Loads;

public sealed class LoadFile
{
    private LoadFile()
    {
    }

    private LoadFile(
        Guid id,
        string fileName,
        string userId,
        string userEmail,
        string correlationId,
        DateTimeOffset createdAt)
    {
        Id = id;
        FileName = fileName;
        UserId = userId;
        UserEmail = userEmail;
        CorrelationId = correlationId;
        CreatedAt = createdAt;

        Status = LoadStatus.Pending;
        Result = LoadResult.Pending;
    }

    public Guid Id { get; private set; }

    public string FileName { get; private set; } = null!;

    public string? StoragePath { get; private set; }

    public string? Period { get; private set; }

    public string UserId { get; private set; } = null!;

    public string UserEmail { get; private set; } = null!;

    public LoadStatus Status { get; private set; }

    public LoadResult Result { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? ProcessingStartedAt { get; private set; }

    public DateTimeOffset? LoadedAt { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public DateTimeOffset? NotifiedAt { get; private set; }

    public int TotalRows { get; private set; }

    public int ValidRows { get; private set; }

    public int InsertedRows { get; private set; }

    public int ExistingRows { get; private set; }

    public int InvalidRows { get; private set; }

    public string CorrelationId { get; private set; } = null!;

    public string? ErrorMessage { get; private set; }

    public static LoadFile Create(
        string fileName,
        string userId,
        string userEmail,
        string correlationId,
        DateTimeOffset? createdAt = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        ArgumentException.ThrowIfNullOrWhiteSpace(userEmail);
        ArgumentException.ThrowIfNullOrWhiteSpace(correlationId);

        return new LoadFile(
            Guid.NewGuid(),
            fileName.Trim(),
            userId.Trim(),
            userEmail.Trim(),
            correlationId.Trim(),
            createdAt ?? DateTimeOffset.UtcNow);
    }

    public void SetStoragePath(string storagePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storagePath);

        StoragePath = storagePath.Trim();
    }

    public void MarkProcessing(
        string period,
        DateTimeOffset? startedAt = null)
    {
        EnsureStatus(LoadStatus.Pending);

        ArgumentException.ThrowIfNullOrWhiteSpace(period);

        Period = period.Trim();
        Status = LoadStatus.Processing;
        ProcessingStartedAt = startedAt ?? DateTimeOffset.UtcNow;
    }

    public void MarkLoaded(
        int totalRows,
        int validRows,
        int insertedRows,
        int existingRows,
        int invalidRows,
        DateTimeOffset? loadedAt = null)
    {
        EnsureStatus(LoadStatus.Processing);

        TotalRows = totalRows;
        ValidRows = validRows;
        InsertedRows = insertedRows;
        ExistingRows = existingRows;
        InvalidRows = invalidRows;

        Status = LoadStatus.Loaded;
        LoadedAt = loadedAt ?? DateTimeOffset.UtcNow;
    }

    public void MarkCompleted(
        LoadResult result,
        DateTimeOffset? completedAt = null)
    {
        EnsureStatus(LoadStatus.Loaded);

        if (result is LoadResult.Pending or LoadResult.Rejected or LoadResult.Failed)
        {
            throw new InvalidOperationException(
                $"Result '{result}' is not valid for a successfully loaded file.");
        }

        Result = result;
        Status = LoadStatus.Completed;
        CompletedAt = completedAt ?? DateTimeOffset.UtcNow;
    }

    public void Reject(
        string errorMessage,
        DateTimeOffset? completedAt = null)
    {
        if (Status is not LoadStatus.Pending and not LoadStatus.Processing)
        {
            throw new InvalidOperationException(
                $"Load cannot be rejected from status '{Status}'.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage);

        Result = LoadResult.Rejected;
        Status = LoadStatus.Completed;
        ErrorMessage = errorMessage.Trim();
        CompletedAt = completedAt ?? DateTimeOffset.UtcNow;
    }

    public void Fail(
        string errorMessage,
        DateTimeOffset? completedAt = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(errorMessage);

        Result = LoadResult.Failed;
        Status = LoadStatus.Completed;
        ErrorMessage = errorMessage.Trim();
        CompletedAt = completedAt ?? DateTimeOffset.UtcNow;
    }

    public void MarkNotified(DateTimeOffset? notifiedAt = null)
    {
        EnsureStatus(LoadStatus.Completed);

        Status = LoadStatus.Notified;
        NotifiedAt = notifiedAt ?? DateTimeOffset.UtcNow;
    }

    private void EnsureStatus(LoadStatus expected)
    {
        if (Status != expected)
        {
            throw new InvalidOperationException(
                $"Invalid load state transition. Expected '{expected}', current '{Status}'.");
        }
    }
}