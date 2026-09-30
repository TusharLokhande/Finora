using MoneyManagement.Application.Features.Budgets.Dto;

namespace MoneyManagement.Application.Features.Budgets;

/// <summary>Percent-used and budget state (docs: warning at 80%, over at 100%). No I/O, easy to unit test.</summary>
public static class BudgetMath
{
    public const int WarningPercent = 80;
    public const int OverPercent = 100;

    /// <summary>spent / budgeted as a percentage, 1 decimal, for display. Null for a zero budget.</summary>
    public static decimal? PercentUsed(long spent, long budgeted)
        => budgeted == 0 ? null : Math.Round(spent * 100m / budgeted, 1, MidpointRounding.AwayFromZero);

    /// <summary>
    /// Compared on exact integers, not the rounded percentage, so 79.96% is still Normal.
    /// A zero budget is Over as soon as anything is spent.
    /// </summary>
    public static BudgetStatus Status(long spent, long budgeted)
    {
        if (budgeted == 0)
            return spent > 0 ? BudgetStatus.Over : BudgetStatus.Normal;
        if (spent * 100 >= budgeted * OverPercent)
            return BudgetStatus.Over;
        if (spent * 100 >= budgeted * WarningPercent)
            return BudgetStatus.Warning;
        return BudgetStatus.Normal;
    }
}
