namespace MoneyManagement.Application.Common.Export;

public enum ExcelFormat
{
    Text,
    Date,
    /// <summary>Minor units (long), written as major units with 2 decimals.</summary>
    Money,
    /// <summary>A percentage value such as 25.5, written as a number with 1 decimal.</summary>
    Percent,
}

public record ExcelColumn(string Header, ExcelFormat Format = ExcelFormat.Text);

/// <summary>One worksheet: typed columns and rows of cell values (string, DateOnly, long, decimal or null).</summary>
public record ExcelSheet(string Name, IReadOnlyList<ExcelColumn> Columns, IReadOnlyList<object?[]> Rows);
