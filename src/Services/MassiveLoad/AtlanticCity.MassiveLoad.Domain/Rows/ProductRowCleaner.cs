using System.Globalization;

namespace AtlanticCity.MassiveLoad.Domain.Rows;

public static class ProductRowCleaner
{
    private const string DefaultText =
        "N/A";

    public static ProductRowCleanResult Clean(
        Guid loadId,
        RawProductRow row,
        string resolvedPeriod)
    {
        ArgumentNullException.ThrowIfNull(
            row);

        if (loadId == Guid.Empty)
        {
            throw new ArgumentException(
                "Load id cannot be empty.",
                nameof(loadId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(
            resolvedPeriod);

        if (row.IsCompletelyEmpty)
        {
            return ProductRowCleanResult.Ignored();
        }

        var period =
            Normalize(row.Period)
            ?? resolvedPeriod.Trim();

        var productCode =
            Normalize(row.ProductCode)
            ?? $"SIN-CODIGO-{loadId:N}-{row.RowNumber}";

        var productName =
            Normalize(row.ProductName)
            ?? DefaultText;

        var description =
            Normalize(row.Description)
            ?? DefaultText;

        var category =
            Normalize(row.Category)
            ?? DefaultText;

        var errors =
            new List<RowProcessingError>();

        ValidateLength(
            row,
            nameof(row.Period),
            period,
            20,
            errors);

        ValidateLength(
            row,
            nameof(row.ProductCode),
            productCode,
            100,
            errors);

        ValidateLength(
            row,
            nameof(row.ProductName),
            productName,
            250,
            errors);

        ValidateLength(
            row,
            nameof(row.Description),
            description,
            500,
            errors);

        ValidateLength(
            row,
            nameof(row.Category),
            category,
            150,
            errors);

        var quantity =
            ParseQuantity(
                row,
                errors);

        var price =
            ParsePrice(
                row,
                errors);

        if (errors.Count > 0)
        {
            return ProductRowCleanResult.Invalid(
                errors);
        }

        return ProductRowCleanResult.Success(
            new ProcessedProduct(
                row.RowNumber,
                period,
                productCode,
                productName,
                description,
                category,
                quantity,
                price));
    }

    private static int ParseQuantity(
        RawProductRow row,
        ICollection<RowProcessingError> errors)
    {
        var value =
            Normalize(row.Quantity);

        if (value is null)
        {
            return 0;
        }

        if (!int.TryParse(
                value,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var quantity) ||
            quantity < 0)
        {
            errors.Add(
                new RowProcessingError(
                    row.RowNumber,
                    "INVALID_QUANTITY",
                    "Cantidad",
                    "Cantidad debe ser un número entero mayor o igual a cero.",
                    row));

            return 0;
        }

        return quantity;
    }

    private static decimal ParsePrice(
        RawProductRow row,
        ICollection<RowProcessingError> errors)
    {
        var value =
            Normalize(row.Price);

        if (value is null)
        {
            return 0m;
        }

        if (!decimal.TryParse(
                value,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var price) ||
            price < 0)
        {
            errors.Add(
                new RowProcessingError(
                    row.RowNumber,
                    "INVALID_PRICE",
                    "Precio",
                    "Precio debe ser un número decimal mayor o igual a cero.",
                    row));

            return 0m;
        }

        return price;
    }

    private static void ValidateLength(
        RawProductRow row,
        string field,
        string value,
        int maxLength,
        ICollection<RowProcessingError> errors)
    {
        if (value.Length <= maxLength)
        {
            return;
        }

        var fieldLabel =
            field switch
            {
                nameof(RawProductRow.Period) =>
                    "Periodo",

                nameof(RawProductRow.ProductCode) =>
                    "CodigoProducto",

                nameof(RawProductRow.ProductName) =>
                    "NombreProducto",

                nameof(RawProductRow.Description) =>
                    "Descripcion",

                nameof(RawProductRow.Category) =>
                    "Categoria",

                _ => field
            };

        errors.Add(
            new RowProcessingError(
                row.RowNumber,
                "MAX_LENGTH_EXCEEDED",
                field,
                $"{fieldLabel} supera la longitud máxima permitida de {maxLength} caracteres.",
                row));
            }

    private static string? Normalize(
        string? value)
    {
        return string.IsNullOrWhiteSpace(
                value)
            ? null
            : value.Trim();
    }
}