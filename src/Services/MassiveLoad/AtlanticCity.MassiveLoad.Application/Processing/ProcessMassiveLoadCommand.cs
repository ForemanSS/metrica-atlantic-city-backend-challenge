namespace AtlanticCity.MassiveLoad.Application.Processing;

public sealed record ProcessMassiveLoadCommand(
    Guid LoadId,
    string StoragePath,
    string FileName,
    string UserId,
    string UserEmail,
    string CorrelationId,
    DateTimeOffset RequestedAt);