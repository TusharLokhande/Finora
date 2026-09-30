using MoneyManagement.Application.Features.Budgets.Dto;

namespace MoneyManagement.Application.Features.Reports.Dto;

/// <summary>
/// Each category's budget and spend summed over the months in the range where it has a budget row.
/// Built from the Budgets page's month numbers, so it matches them summed by hand.
/// </summary>
public class BudgetVsActualDto
{
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }

    /// <summary>Months in the range with at least one budget.</summary>
    public int MonthsWithBudgets { get; set; }

    public long TotalBudgeted { get; set; }

    /// <summary>Each transaction counted once even when a category and its sub-category are both budgeted.</summary>
    public long TotalSpent { get; set; }

    /// <summary>Only categories with at least one budget row; most used first.</summary>
    public IReadOnlyList<BudgetLineDto> Lines { get; set; } = [];
}
