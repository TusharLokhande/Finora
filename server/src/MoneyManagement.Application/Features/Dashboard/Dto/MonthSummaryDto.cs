namespace MoneyManagement.Application.Features.Dashboard.Dto;

/// <summary>One month's headline numbers, in minor units. Transfers are excluded.</summary>
public class MonthSummaryDto
{
    /// <summary>First day of the month.</summary>
    public DateOnly Month { get; set; }
    public long Spent { get; set; }
    public long Income { get; set; }
    public long NetSavings { get; set; }

    /// <summary>Sum of the month's category budgets.</summary>
    public long Budgeted { get; set; }

    /// <summary>Budgeted minus spending in the budgeted categories. Negative when over.</summary>
    public long BudgetRemaining { get; set; }
}
