namespace AtlanticCity.Control.Application.Loads.Upload;

public sealed record LoadUploadSettings(
    long MaxFileSizeBytes)
{
    public const long DefaultMaxFileSizeBytes =
        20L * 1024L * 1024L;

    public static LoadUploadSettings Default =>
        new(DefaultMaxFileSizeBytes);
}