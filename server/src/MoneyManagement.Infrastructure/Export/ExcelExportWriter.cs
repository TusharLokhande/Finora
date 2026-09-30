using ClosedXML.Excel;
using MoneyManagement.Application.Common.Export;
using MoneyManagement.Application.Common.Interfaces;

namespace MoneyManagement.Infrastructure.Export;

public class ExcelExportWriter : IExcelExportWriter
{
    public Task WriteAsync(IReadOnlyList<ExcelSheet> sheets, Stream output, CancellationToken cancellationToken = default)
    {
        using var workbook = new XLWorkbook();

        foreach (var sheet in sheets)
        {
            var ws = workbook.Worksheets.Add(sheet.Name);

            for (var c = 0; c < sheet.Columns.Count; c++)
            {
                ws.Cell(1, c + 1).Value = sheet.Columns[c].Header;
                ws.Column(c + 1).Style.NumberFormat.Format = sheet.Columns[c].Format switch
                {
                    ExcelFormat.Money => "#,##0.00",
                    ExcelFormat.Percent => "0.0",
                    _ => "General",
                };
                if (sheet.Columns[c].Format == ExcelFormat.Date)
                    ws.Column(c + 1).Style.DateFormat.Format = "yyyy-mm-dd";
            }
            ws.Row(1).Style.Font.Bold = true;

            for (var r = 0; r < sheet.Rows.Count; r++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                for (var c = 0; c < sheet.Columns.Count; c++)
                    ws.Cell(r + 2, c + 1).Value = ToCell(sheet.Rows[r][c], sheet.Columns[c].Format);
            }

            ws.SheetView.FreezeRows(1);
            ws.Columns().AdjustToContents();
        }

        workbook.SaveAs(output);
        return Task.CompletedTask;
    }

    private static XLCellValue ToCell(object? value, ExcelFormat format) => value switch
    {
        null => Blank.Value,
        DateOnly d => d.ToDateTime(TimeOnly.MinValue),
        long minor when format == ExcelFormat.Money => minor / 100m,
        long l => l,
        decimal m => m,
        int i => i,
        _ => value.ToString(),
    };
}
