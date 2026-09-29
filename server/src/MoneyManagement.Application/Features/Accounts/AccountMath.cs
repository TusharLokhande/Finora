namespace MoneyManagement.Application.Features.Accounts;

/// <summary>Pure derivation math from docs/data-model.md section 4. No I/O, easy to unit test.</summary>
public static class AccountMath
{
    public static long ComputeBalance(long openingBalance, long delta) => openingBalance + delta;

    public static long ComputeOutstanding(long balance) => -balance;

    public static long ComputeAvailableCredit(long creditLimit, long outstanding) => creditLimit - outstanding;

    /// <summary>
    /// Next due date for a card: this month's due day if it's still upcoming, or already passed
    /// while money is still owed (shown as overdue); otherwise next month's due day.
    /// DueDay is constrained to 1-28, so AddMonths never needs end-of-month clamping.
    /// </summary>
    public static DateOnly ComputeNextDueDate(short dueDay, long outstanding, DateOnly today)
    {
        var dueThisMonth = new DateOnly(today.Year, today.Month, dueDay);

        if (outstanding > 0)
            return dueThisMonth;

        return dueThisMonth < today ? dueThisMonth.AddMonths(1) : dueThisMonth;
    }
}
