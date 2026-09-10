namespace AtlanticCity.MassiveLoad.Application.Persistence;

public enum LoadProcessingResult
{
    Pending = 1,
    Success = 2,
    Partial = 3,
    Rejected = 4,
    Failed = 5
}