namespace AtlanticCity.MassiveLoad.Application.Excel;

public sealed class ExcelLoadException(
    string errorCode,
    string message)
    : Exception(message)
{
    public string ErrorCode { get; } =
        errorCode;
}