namespace MoneyManagement.Application.Features.Reports.Dto;

/// <summary>Totals for the range and the equally long period right before it. Transfers excluded.</summary>
public class ReportSummaryDto
{
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
    public DateOnly PriorFrom { get; set; }
    public DateOnly PriorTo { get; set; }

    public long Spent { get; set; }
    public long Income { get; set; }
    public long NetSavings { get; set; }

    public long PriorSpent { get; set; }
    public long PriorIncome { get; set; }
    public long PriorNetSavings { get; set; }

    /// <summary>Percent changes vs the prior period, 1 decimal; null when the prior value is zero.</summary>
    public decimal? SpentChangePercent { get; set; }
    public decimal? IncomeChangePercent { get; set; }
    public decimal? NetSavingsChangePercent { get; set; }

    /// <summary>Net savings as a share of income.</summary>
    public decimal? SavingsRatePercent { get; set; }
}
