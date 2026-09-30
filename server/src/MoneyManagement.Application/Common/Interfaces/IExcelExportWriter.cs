using MoneyManagement.Application.Common.Export;

namespace MoneyManagement.Application.Common.Interfaces;

/// <summary>Writes one .xlsx workbook with a sheet per <see cref="ExcelSheet"/>. Implemented in Infrastructure.</summary>
public interface IExcelExportWriter
{
    Task WriteAsync(IReadOnlyList<ExcelSheet> sheets, Stream output, CancellationToken cancellationToken = default);
}
