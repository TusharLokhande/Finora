namespace MoneyManagement.Application.Features.Dashboard.Dto;

/// <summary>This month vs last month. Percentages are rounded to 1 decimal; null when the base is zero.</summary>
public class DashboardSummaryDto
{
    public MonthSummaryDto Current { get; set; } = new();
    public MonthSummaryDto Previous { get; set; } = new();

    public decimal? SpentChangePercent { get; set; }
    public decimal? IncomeChangePercent { get; set; }
    public decimal? NetSavingsChangePercent { get; set; }

    /// <summary>Current net savings as a share of current income.</summary>
    public decimal? SavingsRatePercent { get; set; }
}
