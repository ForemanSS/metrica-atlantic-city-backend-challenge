namespace AtlanticCity.BuildingBlocks.Observability;

public static class CorrelationIdHeader
{
    public const string Name =
        "X-Correlation-Id";

    public const int MaxLength =
        100;
}