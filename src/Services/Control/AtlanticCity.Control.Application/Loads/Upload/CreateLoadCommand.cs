namespace AtlanticCity.Control.Application.Loads.Upload;

public sealed record CreateLoadCommand(
    string FileName,
    string? ContentType,
    long FileLength,
    Stream Content,
    string UserId,
    string UserEmail,
    string CorrelationId);