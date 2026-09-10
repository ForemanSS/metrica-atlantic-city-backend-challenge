namespace AtlanticCity.MassiveLoad.Application.Persistence;

public enum LoadProcessingState
{
    Pending = 1,
    Processing = 2,
    Loaded = 3,
    Completed = 4,
    Notified = 5
}