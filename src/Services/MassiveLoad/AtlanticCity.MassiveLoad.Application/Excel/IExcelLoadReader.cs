using AtlanticCity.MassiveLoad.Domain.Rows;

namespace AtlanticCity.MassiveLoad.Application.Excel;

public interface IExcelLoadReader
{
    Task<IReadOnlyCollection<RawProductRow>> ReadAsync(
        Stream content,
        CancellationToken cancellationToken = default);
}