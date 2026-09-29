using MoneyManagement.Application.Features.Accounts;

namespace MoneyManagement.Application.Tests.Features.Accounts;

public class AccountMathTests
{
    [Theory]
    [InlineData(10_000, 0, 10_000)]
    [InlineData(10_000, 5_000, 15_000)]
    [InlineData(10_000, -3_000, 7_000)]
    public void ComputeBalance_AddsDeltaToOpeningBalance(long opening, long delta, long expected)
    {
        Assert.Equal(expected, AccountMath.ComputeBalance(opening, delta));
    }

    [Theory]
    [InlineData(-5_000, 5_000)]
    [InlineData(0, 0)]
    [InlineData(2_000, -2_000)]
    public void ComputeOutstanding_IsNegatedBalance(long balance, long expectedOutstanding)
    {
        Assert.Equal(expectedOutstanding, AccountMath.ComputeOutstanding(balance));
    }

    [Fact]
    public void ComputeAvailableCredit_IsLimitMinusOutstanding()
    {
        Assert.Equal(70_000, AccountMath.ComputeAvailableCredit(creditLimit: 100_000, outstanding: 30_000));
    }

    [Fact]
    public void ComputeNextDueDate_WhenOwed_UsesThisMonthsDueDayEvenIfPast()
    {
        var today = new DateOnly(2026, 9, 29);

        var dueDate = AccountMath.ComputeNextDueDate(dueDay: 15, outstanding: 5_000, today);

        Assert.Equal(new DateOnly(2026, 9, 15), dueDate); // overdue, not rolled forward
    }

    [Fact]
    public void ComputeNextDueDate_WhenOwed_UsesThisMonthsDueDayIfUpcoming()
    {
        var today = new DateOnly(2026, 9, 5);

        var dueDate = AccountMath.ComputeNextDueDate(dueDay: 15, outstanding: 5_000, today);

        Assert.Equal(new DateOnly(2026, 9, 15), dueDate);
    }

    [Fact]
    public void ComputeNextDueDate_WhenNothingOwedAndDueDayPassed_RollsToNextMonth()
    {
        var today = new DateOnly(2026, 9, 29);

        var dueDate = AccountMath.ComputeNextDueDate(dueDay: 15, outstanding: 0, today);

        Assert.Equal(new DateOnly(2026, 10, 15), dueDate);
    }

    [Fact]
    public void ComputeNextDueDate_WhenNothingOwedAndDueDayUpcoming_StaysThisMonth()
    {
        var today = new DateOnly(2026, 9, 5);

        var dueDate = AccountMath.ComputeNextDueDate(dueDay: 15, outstanding: 0, today);

        Assert.Equal(new DateOnly(2026, 9, 15), dueDate);
    }
}
