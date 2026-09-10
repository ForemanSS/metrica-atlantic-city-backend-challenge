namespace AtlanticCity.MassiveLoad.Domain.Rows;

public sealed record ProductRowCleanResult(
    bool IsIgnored,
    ProcessedProduct? Product,
    IReadOnlyCollection<RowProcessingError> Errors)
{
    public bool IsValid =>
        !IsIgnored &&
        Product is not null &&
        Errors.Count == 0;

    public static ProductRowCleanResult Ignored()
    {
        return new ProductRowCleanResult(
            true,
            null,
            []);
    }

    public static ProductRowCleanResult Success(
        ProcessedProduct product)
    {
        return new ProductRowCleanResult(
            false,
            product,
            []);
    }

    public static ProductRowCleanResult Invalid(
        IReadOnlyCollection<RowProcessingError> errors)
    {
        return new ProductRowCleanResult(
            false,
            null,
            errors);
    }
}