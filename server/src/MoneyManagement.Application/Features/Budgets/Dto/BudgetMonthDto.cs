namespace MoneyManagement.Application.Features.Budgets.Dto;

/// <summary>Budget vs actual for one month. The same numbers back the Home dashboard's budget widgets.</summary>
public class BudgetMonthDto
{
    /// <summary>First day of the month.</summary>
    public DateOnly Month { get; set; }

    /// <summary>Sum of the month's category budgets (there is no overall cap).</summary>
    public long TotalBudgeted { get; set; }

    /// <summary>Spending in budgeted categories (a top-level budget covers its sub-categories), each transaction counted once.</summary>
    public long TotalSpent { get; set; }

    public long Remaining => TotalBudgeted - TotalSpent;

    /// <summary>Category order: each top-level category, then its budgeted sub-categories.</summary>
    public IReadOnlyList<BudgetLineDto> Lines { get; set; } = [];
}
