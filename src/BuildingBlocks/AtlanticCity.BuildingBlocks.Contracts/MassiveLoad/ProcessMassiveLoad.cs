namespace AtlanticCity.BuildingBlocks.Contracts.MassiveLoad;

public sealed record ProcessMassiveLoad(
    Guid LoadId,
    string StoragePath,
    string FileName,
    string UserId,
    string UserEmail,
    string CorrelationId,
    DateTimeOffset RequestedAt);