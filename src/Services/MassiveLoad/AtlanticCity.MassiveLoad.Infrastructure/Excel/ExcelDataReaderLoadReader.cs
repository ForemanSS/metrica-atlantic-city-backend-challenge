using System.Globalization;
using AtlanticCity.MassiveLoad.Application.Excel;
using AtlanticCity.MassiveLoad.Domain.Rows;
using ExcelDataReader;
using Microsoft.Extensions.Logging;
using System.Text;

namespace AtlanticCity.MassiveLoad.Infrastructure.Excel;

internal sealed class ExcelDataReaderLoadReader(
    ILogger<ExcelDataReaderLoadReader> logger)
    : IExcelLoadReader
{
    static ExcelDataReaderLoadReader()
    {
        Encoding.RegisterProvider(
            CodePagesEncodingProvider.Instance);
    }

    private static readonly string[] RequiredHeaders =
    [
        "Periodo",
        "CodigoProducto",
        "NombreProducto",
        "Descripcion",
        "Categoria",
        "Cantidad",
        "Precio"
    ];

    public Task<IReadOnlyCollection<RawProductRow>> ReadAsync(
        Stream content,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            content);

        if (!content.CanRead)
        {
            throw new ExcelLoadException(
                "INVALID_EXCEL",
                "No se puede leer el contenido del archivo Excel.");
        }

        if (content.CanSeek)
        {
            content.Position = 0;
        }

        try
        {
            using var reader =
                ExcelReaderFactory.CreateReader(
                    content);

            var rows =
                ReadFirstWorksheet(
                    reader,
                    cancellationToken);

            logger.LogInformation(
                "Excel file read successfully. Rows detected {RowCount}",
                rows.Count);

            return Task.FromResult<
                IReadOnlyCollection<RawProductRow>>(
                    rows);
        }
        catch (ExcelLoadException)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Excel file could not be read.");

            throw new ExcelLoadException(
                "INVALID_EXCEL",
                "El archivo cargado no es un documento .xlsx válido.");
        }
    }

    private static IReadOnlyCollection<RawProductRow>
        ReadFirstWorksheet(
            IExcelDataReader reader,
            CancellationToken cancellationToken)
    {
        cancellationToken
            .ThrowIfCancellationRequested();

        if (!reader.Read())
        {
            throw new ExcelLoadException(
                "EMPTY_WORKSHEET",
                "La primera hoja del archivo Excel está vacía.");
        }

        var headerMap =
            ReadHeaders(
                reader);

        ValidateHeaders(
            headerMap);

        var rows =
            new List<RawProductRow>();

        var excelRowNumber =
            1;

        while (reader.Read())
        {
            cancellationToken
                .ThrowIfCancellationRequested();

            excelRowNumber++;

            var row =
                new RawProductRow(
                    excelRowNumber,
                    GetCell(
                        reader,
                        headerMap["Periodo"]),
                    GetCell(
                        reader,
                        headerMap["CodigoProducto"]),
                    GetCell(
                        reader,
                        headerMap["NombreProducto"]),
                    GetCell(
                        reader,
                        headerMap["Descripcion"]),
                    GetCell(
                        reader,
                        headerMap["Categoria"]),
                    GetCell(
                        reader,
                        headerMap["Cantidad"]),
                    GetCell(
                        reader,
                        headerMap["Precio"]));

            rows.Add(
                row);
        }

        return rows;
    }

    private static Dictionary<string, int>
        ReadHeaders(
            IExcelDataReader reader)
    {
        var headers =
            new Dictionary<string, int>(
                StringComparer.OrdinalIgnoreCase);

        for (var columnIndex = 0;
             columnIndex < reader.FieldCount;
             columnIndex++)
        {
            var header =
                reader.GetValue(
                        columnIndex)?
                    .ToString()?
                    .Trim();

            if (string.IsNullOrWhiteSpace(
                    header))
            {
                continue;
            }

            if (!headers.TryAdd(
                    header,
                    columnIndex))
            {
                throw new ExcelLoadException(
                    "DUPLICATE_COLUMN",
                    $"El archivo Excel contiene la columna duplicada '{header}'.");
            }
        }

        return headers;
    }

    private static void ValidateHeaders(
        IReadOnlyDictionary<string, int> headers)
    {
        var missingHeaders =
            RequiredHeaders
                .Where(
                    header =>
                        !headers.ContainsKey(
                            header))
                .ToArray();

        if (missingHeaders.Length == 0)
        {
            return;
        }

        throw new ExcelLoadException(
            "MISSING_COLUMNS",
            $"El archivo Excel no contiene las columnas obligatorias: {string.Join(", ", missingHeaders)}.");
    }

    private static string? GetCell(
        IExcelDataReader reader,
        int columnIndex)
    {
        if (reader.IsDBNull(
                columnIndex))
        {
            return null;
        }

        var value =
            reader.GetValue(
                columnIndex);

        if (value is null)
        {
            return null;
        }

        var text =
            value switch
            {
                DateTime dateTime =>
                    dateTime.ToString(
                        "yyyy-MM-dd",
                        CultureInfo.InvariantCulture),

                double number =>
                    number.ToString(
                        "G17",
                        CultureInfo.InvariantCulture),

                float number =>
                    number.ToString(
                        "G9",
                        CultureInfo.InvariantCulture),

                decimal number =>
                    number.ToString(
                        CultureInfo.InvariantCulture),

                IFormattable formattable =>
                    formattable.ToString(
                        null,
                        CultureInfo.InvariantCulture),

                _ =>
                    value.ToString()
            };

        return string.IsNullOrWhiteSpace(
                text)
            ? null
            : text.Trim();
    }
}