using MoneyManagement.Application.Common;
using MoneyManagement.Application.Tests.Common;

namespace MoneyManagement.Application.Tests.Features.Dashboard;

public class DashboardServiceTests
{
    private static readonly DateOnly Today = new(2026, 9, 30);
    private static readonly DateOnly Sep = new(2026, 9, 1);
    private static readonly DateOnly Aug = new(2026, 8, 1);

    [Fact]
    public async Task Summary_ComparesThisMonthWithLastMonth()
    {
        var ledger = new TestLedger(Today);
        ledger.Income(40_000, new DateOnly(2026, 8, 1));
        ledger.Expense(20_000, ledger.Transport, new DateOnly(2026, 8, 20));

        ledger.Income(45_000, new DateOnly(2026, 9, 1));
        ledger.Expense(1_000, ledger.Food, new DateOnly(2026, 9, 2));
        ledger.Expense(3_000, ledger.Groceries, new DateOnly(2026, 9, 3));
        ledger.Expense(5_000, ledger.DiningOut, new DateOnly(2026, 9, 4));
        ledger.Expense(9_000, ledger.Transport, new DateOnly(2026, 9, 30));
        ledger.Transfer(10_000, new DateOnly(2026, 9, 15)); // never counted
        ledger.Expense(99_000, ledger.Food, new DateOnly(2026, 10, 1)); // next month, ignored

        var s = (await ledger.Dashboard.GetSummaryAsync(ledger.UserId)).Data!;

        Assert.Equal(Sep, s.Current.Month);
        Assert.Equal(18_000, s.Current.Spent);
        Assert.Equal(45_000, s.Current.Income);
        Assert.Equal(27_000, s.Current.NetSavings);

        Assert.Equal(Aug, s.Previous.Month);
        Assert.Equal(20_000, s.Previous.Spent);
        Assert.Equal(40_000, s.Previous.Income);
        Assert.Equal(20_000, s.Previous.NetSavings);

        Assert.Equal(-10m, s.SpentChangePercent);
        Assert.Equal(12.5m, s.IncomeChangePercent);
        Assert.Equal(35m, s.NetSavingsChangePercent);
        Assert.Equal(60m, s.SavingsRatePercent);
    }

    [Fact]
    public async Task Summary_BudgetRemainingIsBudgetsMinusSpendInThoseCategories()
    {
        var ledger = new TestLedger(Today);
        ledger.Budget(ledger.Food, Sep, 10_000);
        ledger.Budget(ledger.Transport, Sep, 5_000);
        ledger.Budget(ledger.Food, Aug, 4_000);

        ledger.Expense(1_000, ledger.Food, new DateOnly(2026, 9, 2));
        ledger.Expense(3_000, ledger.Groceries, new DateOnly(2026, 9, 3)); // rolls into Food
        ledger.Expense(9_000, ledger.Transport, new DateOnly(2026, 9, 4));
        ledger.Expense(3_000, ledger.Food, new DateOnly(2026, 8, 4));

        var s = (await ledger.Dashboard.GetSummaryAsync(ledger.UserId)).Data!;

        Assert.Equal(15_000, s.Current.Budgeted);
        Assert.Equal(15_000 - (4_000 + 9_000), s.Current.BudgetRemaining);
        Assert.Equal(4_000, s.Previous.Budgeted);
        Assert.Equal(1_000, s.Previous.BudgetRemaining);
    }

    [Fact]
    public async Task Summary_ChangeIsNullWhenLastMonthWasZero()
    {
        var ledger = new TestLedger(Today);
        ledger.Expense(1_000, ledger.Food, new DateOnly(2026, 9, 2));

        var s = (await ledger.Dashboard.GetSummaryAsync(ledger.UserId)).Data!;

        Assert.Null(s.SpentChangePercent);
        Assert.Null(s.IncomeChangePercent);
        Assert.Null(s.SavingsRatePercent);
        Assert.Equal(0, s.Current.Budgeted);
        Assert.Equal(0, s.Current.BudgetRemaining);
    }

    [Fact]
    public async Task BudgetsAtRisk_IncludesOnlyEightyPercentAndUp_MostUsedFirst()
    {
        var ledger = new TestLedger(Today);
        ledger.Budget(ledger.Food, Sep, 100_000);      // 9.2% -> out
        ledger.Budget(ledger.Groceries, Sep, 10_000);  // 79.9% -> out
        ledger.Budget(ledger.Transport, Sep, 1_000);   // exactly 80% -> in
        ledger.Budget(ledger.DiningOut, Sep, 1_000);   // 125% -> in, first
        ledger.Budget(ledger.Transport, Aug, 1);       // last month -> out

        ledger.Expense(7_990, ledger.Groceries, new DateOnly(2026, 9, 3));
        ledger.Expense(1_250, ledger.DiningOut, new DateOnly(2026, 9, 3));
        ledger.Expense(800, ledger.Transport, new DateOnly(2026, 9, 3));
        ledger.Expense(50_000, ledger.Transport, new DateOnly(2026, 8, 3));

        var atRisk = (await ledger.Dashboard.GetBudgetsAtRiskAsync(ledger.UserId)).Data!;

        Assert.Equal(["Dining out", "Transport"], atRisk.Select(b => b.CategoryName));
        Assert.Equal([125m, 80m], atRisk.Select(b => b.PercentUsed));
        Assert.Equal("Food", atRisk[0].ParentCategoryName);
        Assert.Equal(1_250, atRisk[0].Spent);
    }

    [Fact]
    public async Task TopCategories_RollUpToParentsAndRankBySpend()
    {
        var ledger = new TestLedger(Today);
        ledger.Expense(1_000, ledger.Food, new DateOnly(2026, 9, 2));
        ledger.Expense(3_000, ledger.Groceries, new DateOnly(2026, 9, 3));
        ledger.Expense(5_000, ledger.DiningOut, new DateOnly(2026, 9, 4));
        ledger.Expense(6_000, ledger.Transport, new DateOnly(2026, 9, 5));
        ledger.Expense(50_000, ledger.Transport, new DateOnly(2026, 8, 5)); // last month
        ledger.Income(90_000, new DateOnly(2026, 9, 1));                    // not spend
        ledger.Transfer(90_000, new DateOnly(2026, 9, 1));                  // not spend

        var top = (await ledger.Dashboard.GetTopCategoriesAsync(ledger.UserId, 3)).Data!;

        Assert.Equal(["Food", "Transport"], top.Select(c => c.Name));
        Assert.Equal([9_000L, 6_000L], top.Select(c => c.Spent));
        Assert.Equal([60m, 40m], top.Select(c => c.SharePercent));

        var onlyFirst = (await ledger.Dashboard.GetTopCategoriesAsync(ledger.UserId, 1)).Data!;
        Assert.Equal("Food", Assert.Single(onlyFirst).Name);
    }

    [Fact]
    public async Task IncomeExpense_FillsEveryMonthOldestFirst()
    {
        var ledger = new TestLedger(Today);
        ledger.Income(5_000, new DateOnly(2026, 4, 10));
        ledger.Expense(2_000, ledger.Food, new DateOnly(2026, 9, 10));
        ledger.Expense(7_000, ledger.Food, new DateOnly(2026, 3, 31)); // just outside 6 months

        var months = (await ledger.Dashboard.GetIncomeExpenseAsync(ledger.UserId, 6)).Data!;

        Assert.Equal(6, months.Count);
        Assert.Equal(new DateOnly(2026, 4, 1), months[0].Month);
        Assert.Equal(Sep, months[^1].Month);
        Assert.Equal(5_000, months[0].Income);
        Assert.Equal(2_000, months[^1].Expense);
        Assert.Equal(7_000, months.Sum(m => m.Income + m.Expense));
    }

    [Theory]
    [InlineData(1, 3, 33.3)]
    [InlineData(2, 3, 66.7)]
    [InlineData(0, 5, 0)]
    public void Percent_RoundsToOneDecimal(long part, long whole, double expected)
        => Assert.Equal((decimal)expected, PercentMath.Percent(part, whole));

    [Fact]
    public void ChangePercent_UsesTheSizeOfANegativeBase()
    {
        // Net savings went from -1,000 to +500: an improvement, so positive.
        Assert.Equal(150m, PercentMath.ChangePercent(500, -1_000));
        Assert.Null(PercentMath.ChangePercent(500, 0));
    }
}
