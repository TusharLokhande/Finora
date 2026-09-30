using MoneyManagement.Application.Common;
using MoneyManagement.Application.Features.Reports.Requests;
using MoneyManagement.Application.Tests.Common;
using MoneyManagement.Domain.Entities;
using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Application.Tests.Features.Reports;

public class ReportServiceTests
{
    private static readonly DateOnly Today = new(2026, 9, 15);
    private static DateOnly D(int month, int day) => new(2026, month, day);
    private static DateOnly M(int month) => new(2026, month, 1);

    // ---- range resolution ----------------------------------------------------------------

    [Fact]
    public void ThreeMonths_IsThisCalendarMonthPlusThePreviousTwo_NotARolling90Days()
    {
        var (range, error) = DateRangeMath.Resolve(3, null, null, Today);

        Assert.Null(error);
        Assert.Equal(new DateRange(D(7, 1), D(9, 30)), range);
        Assert.NotEqual(Today.AddDays(-89), range.From); // a rolling window would start 18 Jun
    }

    [Theory]
    [InlineData(1, "2026-09-01", "2026-09-30")]
    [InlineData(6, "2026-04-01", "2026-09-30")]
    [InlineData(12, "2025-10-01", "2026-09-30")] // crosses the year boundary
    public void NamedRanges_AreWholeCalendarMonths(int months, string from, string to)
    {
        var (range, _) = DateRangeMath.Resolve(months, null, null, Today);
        Assert.Equal(new DateRange(DateOnly.Parse(from), DateOnly.Parse(to)), range);
    }

    [Fact]
    public void Resolve_DefaultsToSixMonths_AndValidatesCustomRanges()
    {
        Assert.Equal(DateRangeMath.LastMonths(6, Today), DateRangeMath.Resolve(null, null, null, Today).Range);
        Assert.Equal(new DateRange(D(2, 3), D(2, 20)), DateRangeMath.Resolve(3, D(2, 3), D(2, 20), Today).Range); // custom wins
        Assert.NotNull(DateRangeMath.Resolve(null, D(2, 3), null, Today).Error);
        Assert.NotNull(DateRangeMath.Resolve(null, D(3, 1), D(2, 1), Today).Error);
        Assert.NotNull(DateRangeMath.Resolve(0, null, null, Today).Error);
        Assert.NotNull(DateRangeMath.Resolve(25, null, null, Today).Error);
    }

    [Fact]
    public void PriorPeriod_ForNamedRanges_IsTheSameNumberOfCalendarMonths()
    {
        Assert.Equal(new DateRange(D(4, 1), D(6, 30)), DateRangeMath.PriorPeriod(new(D(7, 1), D(9, 30)), 3));
        // One month: March (31 days) -> all of February (28 days), not "31 days back".
        Assert.Equal(new DateRange(D(2, 1), D(2, 28)), DateRangeMath.PriorPeriod(new(D(3, 1), D(3, 31)), 1));
    }

    [Fact]
    public void PriorPeriod_ForCustomRanges_IsTheSameNumberOfDaysJustBefore()
    {
        var prior = DateRangeMath.PriorPeriod(new(D(3, 10), D(3, 19)), null); // 10 days

        Assert.Equal(new DateRange(D(2, 28), D(3, 9)), prior);
        Assert.Equal(10, prior.Days);
    }

    [Fact]
    public void MonthsIn_ListsEveryMonthTheRangeTouches()
    {
        Assert.Equal([M(1), M(2), M(3)], DateRangeMath.MonthsIn(new(D(1, 20), D(3, 5))));
    }

    // ---- summary -------------------------------------------------------------------------

    [Fact]
    public async Task Summary_ComparesWithTheEquallyLongPriorPeriod()
    {
        var ledger = new TestLedger(Today);
        ledger.Income(10_000, D(8, 1));
        ledger.Expense(4_000, ledger.Food, D(9, 2));
        ledger.Transfer(99_000, D(9, 3));               // never counted
        ledger.Income(8_000, D(6, 1));                    // prior period (Jun–Jul)
        ledger.Expense(5_000, ledger.Transport, D(7, 31));
        ledger.Expense(77_000, ledger.Food, D(5, 31));    // before the prior period

        var s = (await ledger.Reports.GetSummaryAsync(ledger.UserId, new ReportRangeRequest { Months = 2 })).Data!;

        Assert.Equal((D(8, 1), D(9, 30)), (s.From, s.To));
        Assert.Equal((D(6, 1), D(7, 31)), (s.PriorFrom, s.PriorTo));
        Assert.Equal((4_000, 10_000, 6_000), (s.Spent, s.Income, s.NetSavings));
        Assert.Equal((5_000, 8_000, 3_000), (s.PriorSpent, s.PriorIncome, s.PriorNetSavings));
        Assert.Equal(-20m, s.SpentChangePercent);
        Assert.Equal(25m, s.IncomeChangePercent);
        Assert.Equal(60m, s.SavingsRatePercent);
    }

    // ---- by category ---------------------------------------------------------------------

    [Fact]
    public async Task ByCategory_ShowsTopNAndBucketsTheRestIntoOther()
    {
        var ledger = new TestLedger(Today);
        var spends = new long[] { 700, 600, 500, 400, 300, 200, 100 };
        for (var i = 0; i < spends.Length; i++)
        {
            var category = new Category { UserId = ledger.UserId, Name = $"C{i}", Type = CategoryType.Expense };
            ledger.Context.Categories.Add(category);
            ledger.Context.SaveChanges();
            ledger.Expense(spends[i], category, D(9, 1));
        }

        var b = (await ledger.Reports.GetByCategoryAsync(ledger.UserId, new ReportRangeRequest { Months = 1 }, top: 5)).Data!;

        Assert.Equal(2_800, b.Total);
        Assert.Equal(["C0", "C1", "C2", "C3", "C4"], b.Categories.Select(c => c.Name));
        Assert.Equal(25m, b.Categories[0].SharePercent);
        Assert.NotNull(b.Other);
        Assert.Equal(2, b.Other!.CategoryCount);
        Assert.Equal(300, b.Other.Spent);
        Assert.Equal(10.7m, b.Other.SharePercent); // 300 / 2800
        Assert.Equal(b.Total, b.Categories.Sum(c => c.Spent) + b.Other.Spent);

        var all = (await ledger.Reports.GetByCategoryAsync(ledger.UserId, new ReportRangeRequest { Months = 1 }, top: 10)).Data!;
        Assert.Null(all.Other);
    }

    [Fact]
    public async Task ByCategory_RollsUpSubCategories_AndSubcategoriesSplitThemBackOut()
    {
        var ledger = new TestLedger(Today);
        ledger.Expense(1_000, ledger.Food, D(9, 1));
        ledger.Expense(3_000, ledger.Groceries, D(9, 1));
        ledger.Expense(4_000, ledger.DiningOut, D(9, 1));
        ledger.Expense(2_000, ledger.Transport, D(9, 1));

        var b = (await ledger.Reports.GetByCategoryAsync(ledger.UserId, new ReportRangeRequest { Months = 1 }, 5)).Data!;
        var food = b.Categories[0];
        Assert.Equal(("Food", 8_000L, true), (food.Name, food.Spent, food.HasSubcategories));
        Assert.False(b.Categories[1].HasSubcategories);

        var subs = (await ledger.Reports.GetSubcategoriesAsync(ledger.UserId, ledger.Food.Id, new ReportRangeRequest { Months = 1 })).Data!;
        Assert.Equal(8_000, subs.Total);
        Assert.Equal(["Dining out", "Groceries", "Food (no sub-category)"], subs.Categories.Select(c => c.Name));
        Assert.Equal([50m, 37.5m, 12.5m], subs.Categories.Select(c => c.SharePercent));

        var notAParent = await ledger.Reports.GetSubcategoriesAsync(ledger.UserId, ledger.Groceries.Id, new ReportRangeRequest { Months = 1 });
        Assert.Equal(ErrorStatus.NotFound, notAParent.ErrorStatus);
    }

    [Fact]
    public async Task IncomeVsExpense_HasOneRowPerCalendarMonthInRange()
    {
        var ledger = new TestLedger(Today);
        ledger.Income(5_000, D(7, 10));
        ledger.Expense(2_000, ledger.Food, D(9, 1));

        var rows = (await ledger.Reports.GetIncomeVsExpenseAsync(ledger.UserId, new ReportRangeRequest { Months = 3 })).Data!;

        Assert.Equal([M(7), M(8), M(9)], rows.Select(r => r.Month));
        Assert.Equal([5_000L, 0, 0], rows.Select(r => r.Income));
        Assert.Equal([0L, 0, 2_000], rows.Select(r => r.Expense));
    }

    // ---- budget vs actual ----------------------------------------------------------------

    [Fact]
    public async Task BudgetVsActual_SumsOnlyBudgetedMonths_WithoutDoubleCounting()
    {
        var ledger = new TestLedger(Today);
        // Food budgeted Jul + Aug; its sub-category Dining out budgeted Aug + Sep. Transport never budgeted.
        ledger.Budget(ledger.Food, M(7), 10_000);
        ledger.Budget(ledger.Food, M(8), 12_000);
        ledger.Budget(ledger.DiningOut, M(8), 3_000);
        ledger.Budget(ledger.DiningOut, M(9), 3_000);

        ledger.Expense(6_000, ledger.Groceries, D(7, 5));  // Food Jul
        ledger.Expense(2_000, ledger.DiningOut, D(8, 5));  // Food Aug + Dining out Aug
        ledger.Expense(5_000, ledger.Food, D(8, 6));       // Food Aug
        ledger.Expense(4_000, ledger.DiningOut, D(9, 5));  // Dining out Sep (Food has no Sep budget)
        ledger.Expense(9_000, ledger.Transport, D(8, 5));  // no budget anywhere

        var r = (await ledger.Reports.GetBudgetVsActualAsync(ledger.UserId, new ReportRangeRequest { Months = 3 })).Data!;
        var byName = r.Lines.ToDictionary(l => l.CategoryName);

        Assert.Equal(["Dining out", "Food"], r.Lines.Select(l => l.CategoryName)); // most used first; Transport omitted
        Assert.Equal((22_000L, 13_000L), (byName["Food"].Budgeted!.Value, byName["Food"].Spent));
        Assert.Equal((6_000L, 6_000L), (byName["Dining out"].Budgeted!.Value, byName["Dining out"].Spent));
        Assert.Equal(100m, byName["Dining out"].PercentUsed);

        Assert.Equal(3, r.MonthsWithBudgets);
        Assert.Equal(28_000, r.TotalBudgeted);
        // Aug's 2,000 Dining out spend is inside both budgets but counts once: 6k + 7k + 4k.
        Assert.Equal(17_000, r.TotalSpent);
    }

    [Fact]
    public async Task BudgetVsActual_MatchesTheBudgetsPageMonthsSummedByHand()
    {
        var ledger = new TestLedger(Today);
        ledger.Budget(ledger.Food, M(8), 12_000);
        ledger.Budget(ledger.Food, M(9), 9_000);
        ledger.Expense(3_000, ledger.Groceries, D(8, 5));
        ledger.Expense(4_500, ledger.Food, D(9, 5));

        var report = (await ledger.Reports.GetBudgetVsActualAsync(ledger.UserId, new ReportRangeRequest { Months = 2 })).Data!;
        var aug = (await ledger.Budgets.GetMonthAsync(ledger.UserId, M(8))).Data!;
        var sep = (await ledger.Budgets.GetMonthAsync(ledger.UserId, M(9))).Data!;

        var food = Assert.Single(report.Lines);
        Assert.Equal(aug.Lines[0].Budgeted + sep.Lines[0].Budgeted, food.Budgeted);
        Assert.Equal(aug.Lines[0].Spent + sep.Lines[0].Spent, food.Spent);
    }

    [Fact]
    public async Task Export_WritesASheetPerSection()
    {
        var ledger = new TestLedger(Today);
        ledger.Expense(1_000, ledger.Food, D(9, 1));
        using var stream = new MemoryStream();

        var result = await ledger.Reports.ExportAsync(ledger.UserId, new ReportRangeRequest { Months = 3 }, stream);

        Assert.True(result.IsSuccess);
        stream.Position = 0;
        using var workbook = new ClosedXML.Excel.XLWorkbook(stream);
        Assert.Equal(["By category", "Income vs expense"], workbook.Worksheets.Select(w => w.Name));
        Assert.Equal(3 + 1, workbook.Worksheet("Income vs expense").RowsUsed().Count()); // header + 3 months
    }
}
