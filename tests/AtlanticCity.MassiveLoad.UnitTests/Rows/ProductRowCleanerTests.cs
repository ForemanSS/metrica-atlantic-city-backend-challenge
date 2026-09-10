using AtlanticCity.MassiveLoad.Domain.Rows;

namespace AtlanticCity.MassiveLoad.UnitTests.Rows;

public sealed class ProductRowCleanerTests
{
    [Fact]
    public void Clean_WhenOptionalValuesAreEmpty_ShouldUseDefaults()
    {
        var loadId =
            Guid.NewGuid();

        var row =
            new RawProductRow(
                2,
                null,
                "PROD-001",
                null,
                null,
                null,
                null,
                null);

        var result =
            ProductRowCleaner.Clean(
                loadId,
                row,
                "2026-09");

        Assert.True(result.IsValid);

        Assert.Equal(
            "2026-09",
            result.Product!.Period);

        Assert.Equal(
            "N/A",
            result.Product.ProductName);

        Assert.Equal(
            "N/A",
            result.Product.Description);

        Assert.Equal(
            "N/A",
            result.Product.Category);

        Assert.Equal(
            0,
            result.Product.Quantity);

        Assert.Equal(
            0m,
            result.Product.Price);
    }

    [Fact]
    public void Clean_WhenProductCodeIsEmpty_ShouldGenerateTraceableDefault()
    {
        var loadId =
            Guid.NewGuid();

        var row =
            new RawProductRow(
                7,
                "2026-09",
                null,
                "Product",
                null,
                null,
                "1",
                "5.50");

        var result =
            ProductRowCleaner.Clean(
                loadId,
                row,
                "2026-09");

        Assert.True(result.IsValid);

        Assert.Equal(
            $"SIN-CODIGO-{loadId:N}-7",
            result.Product!.ProductCode);
    }

    [Fact]
    public void Clean_WhenNumericValueIsInvalid_ShouldReturnError()
    {
        var row =
            new RawProductRow(
                2,
                "2026-09",
                "PROD-001",
                "Product",
                "Description",
                "Category",
                "ABC",
                "-2");

        var result =
            ProductRowCleaner.Clean(
                Guid.NewGuid(),
                row,
                "2026-09");

        Assert.False(result.IsValid);

        Assert.Contains(
            result.Errors,
            x =>
                x.ErrorCode ==
                "INVALID_QUANTITY");

        Assert.Contains(
            result.Errors,
            x =>
                x.ErrorCode ==
                "INVALID_PRICE");
    }

    [Fact]
    public void Clean_WhenRowIsCompletelyEmpty_ShouldIgnoreIt()
    {
        var row =
            new RawProductRow(
                5,
                null,
                null,
                null,
                null,
                null,
                null,
                null);

        var result =
            ProductRowCleaner.Clean(
                Guid.NewGuid(),
                row,
                "2026-09");

        Assert.True(result.IsIgnored);
        Assert.Null(result.Product);
        Assert.Empty(result.Errors);
    }
}