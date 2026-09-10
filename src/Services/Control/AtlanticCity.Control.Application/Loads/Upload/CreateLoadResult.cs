namespace AtlanticCity.Control.Application.Loads.Upload;

public sealed record CreateLoadResult(
    bool IsAccepted,
    Guid? LoadId,
    string? ErrorCode,
    string? ErrorMessage)
{
    public static CreateLoadResult Accepted(
        Guid loadId)
    {
        return new CreateLoadResult(
            true,
            loadId,
            null,
            null);
    }

    public static CreateLoadResult Invalid(
        string errorCode,
        string errorMessage)
    {
        return new CreateLoadResult(
            false,
            null,
            errorCode,
            errorMessage);
    }
}