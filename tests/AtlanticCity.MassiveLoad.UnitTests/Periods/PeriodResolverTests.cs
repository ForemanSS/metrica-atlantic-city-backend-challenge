using AtlanticCity.MassiveLoad.Domain.Periods;
using AtlanticCity.MassiveLoad.Domain.Rows;

namespace AtlanticCity.MassiveLoad.UnitTests.Periods;

public sealed class PeriodResolverTests
{
    [Fact]
    public void Resolve_WhenRowsHaveOnePeriod_ShouldResolveIt()
    {
        var rows =
            new[]
            {
                CreateRow(2, "2026-09"),
                CreateRow(3, null),
                CreateRow(4, " 2026-09 ")
            };

        var result =
            PeriodResolver.Resolve(rows);

        Assert.True(result.IsSuccess);
        Assert.Equal(
            "2026-09",
            result.Period);
    }

    [Fact]
    public void Resolve_WhenRowsHaveDifferentPeriods_ShouldReject()
    {
        var rows =
            new[]
            {
                CreateRow(2, "2026-09"),
                CreateRow(3, "2026-10")
            };

        var result =
            PeriodResolver.Resolve(rows);

        Assert.Equal(
            PeriodResolutionStatus.MultiplePeriods,
            result.Status);

        Assert.Equal(
            "MULTIPLE_PERIODS",
            result.ErrorCode);
    }

    [Fact]
    public void Resolve_WhenPeriodIsMissing_ShouldReject()
    {
        var rows =
            new[]
            {
                CreateRow(2, null)
            };

        var result =
            PeriodResolver.Resolve(rows);

        Assert.Equal(
            PeriodResolutionStatus.MissingPeriod,
            result.Status);

        Assert.Equal(
            "PERIOD_REQUIRED",
            result.ErrorCode);
    }

    [Fact]
    public void Resolve_WhenAllRowsAreEmpty_ShouldReject()
    {
        var rows =
            new[]
            {
                new RawProductRow(
                    2,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null)
            };

        var result =
            PeriodResolver.Resolve(rows);

        Assert.Equal(
            PeriodResolutionStatus.NoData,
            result.Status);
    }

    private static RawProductRow CreateRow(
        int rowNumber,
        string? period)
    {
        return new RawProductRow(
            rowNumber,
            period,
            "PROD-001",
            "Product",
            "Description",
            "Category",
            "1",
            "10.50");
    }
}