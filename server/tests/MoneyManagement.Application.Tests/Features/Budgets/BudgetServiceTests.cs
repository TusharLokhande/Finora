using MoneyManagement.Application.Common;
using MoneyManagement.Application.Features.Budgets;
using MoneyManagement.Application.Features.Budgets.Dto;
using MoneyManagement.Application.Features.Budgets.Requests;
using MoneyManagement.Application.Tests.Common;
using MoneyManagement.Domain.Entities;
using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Application.Tests.Features.Budgets;

public class BudgetServiceTests
{
    private static readonly DateOnly Aug = new(2026, 8, 1);
    private static readonly DateOnly Sep = new(2026, 9, 1);
    private static readonly DateOnly Sep10 = new(2026, 9, 10);

    // ---- percent used and state at the boundaries ----------------------------------------

    [Theory]
    [InlineData(7_900, 79.0, BudgetStatus.Normal)]
    [InlineData(7_999, 80.0, BudgetStatus.Normal)]   // displays as 80.0 but is still below 80%
    [InlineData(8_000, 80.0, BudgetStatus.Warning)]
    [InlineData(9_999, 100.0, BudgetStatus.Warning)] // displays as 100.0 but isn't over yet
    [InlineData(10_000, 100.0, BudgetStatus.Over)]
    [InlineData(10_100, 101.0, BudgetStatus.Over)]
    public void PercentAndStatus_AtTheBoundaries(long spent, double percent, BudgetStatus status)
    {
        Assert.Equal((decimal)percent, BudgetMath.PercentUsed(spent, 10_000));
        Assert.Equal(status, BudgetMath.Status(spent, 10_000));
    }

    [Fact]
    public void ZeroBudget_HasNoPercentAndIsOverOnceSpent()
    {
        Assert.Null(BudgetMath.PercentUsed(500, 0));
        Assert.Equal(BudgetStatus.Normal, BudgetMath.Status(0, 0));
        Assert.Equal(BudgetStatus.Over, BudgetMath.Status(1, 0));
    }

    [Theory]
    [InlineData(7_900, BudgetStatus.Normal)]
    [InlineData(8_000, BudgetStatus.Warning)]
    [InlineData(10_000, BudgetStatus.Over)]
    [InlineData(10_100, BudgetStatus.Over)]
    public async Task GetMonth_ReportsTheSameStateEndToEnd(long spent, BudgetStatus status)
    {
        var ledger = new TestLedger();
        ledger.Budget(ledger.Transport, Sep, 10_000);
        ledger.Expense(spent, ledger.Transport, Sep10);

        var line = (await ledger.Budgets.GetMonthAsync(ledger.UserId, Sep)).Data!.Lines.Single(l => l.CategoryId == ledger.Transport.Id);

        Assert.Equal(spent, line.Spent);
        Assert.Equal(status, line.Status);
    }

    // ---- month listing -------------------------------------------------------------------

    [Fact]
    public async Task GetMonth_ListsEveryExpenseCategory_AndOnlyBudgetedSubCategories()
    {
        var ledger = new TestLedger();
        ledger.Budget(ledger.Food, Sep, 10_000);
        ledger.Budget(ledger.DiningOut, Sep, 3_000);
        ledger.Expense(1_000, ledger.Food, Sep10);
        ledger.Expense(2_000, ledger.Groceries, Sep10);
        ledger.Expense(4_000, ledger.DiningOut, Sep10);
        ledger.Expense(500, ledger.Transport, Sep10);
        ledger.Expense(9_999, ledger.Food, new DateOnly(2026, 8, 31)); // other month

        var month = (await ledger.Budgets.GetMonthAsync(ledger.UserId, Sep)).Data!;
        var byName = month.Lines.ToDictionary(l => l.CategoryName);

        // Groceries has no budget row, Salary is income: neither is listed.
        // Category order, with the budgeted sub-category right under its parent.
        Assert.Equal(["Food", "Dining out", "Transport"], month.Lines.Select(l => l.CategoryName));
        Assert.Equal(ledger.Food.Id, byName["Dining out"].ParentId);

        Assert.Equal(7_000, byName["Food"].Spent);            // own + both sub-categories
        Assert.Equal(70m, byName["Food"].PercentUsed);
        Assert.Equal(4_000, byName["Dining out"].Spent);
        Assert.Equal(BudgetStatus.Over, byName["Dining out"].Status);
        Assert.Null(byName["Transport"].Budgeted);             // listed so the UI can offer "Set budget"
        Assert.Null(byName["Transport"].PercentUsed);

        Assert.Equal(13_000, month.TotalBudgeted);
        Assert.Equal(7_000, month.TotalSpent);                 // Dining out's 4,000 counted once
        Assert.Equal(6_000, month.Remaining);
    }

    [Fact]
    public async Task GetMonth_MatchesTheDashboardNumbers()
    {
        var ledger = new TestLedger(new DateOnly(2026, 9, 30));
        ledger.Budget(ledger.Food, Sep, 10_000);
        ledger.Budget(ledger.Transport, Sep, 1_000);
        ledger.Expense(3_000, ledger.Groceries, Sep10);
        ledger.Expense(900, ledger.Transport, Sep10);

        var month = (await ledger.Budgets.GetMonthAsync(ledger.UserId, Sep)).Data!;
        var summary = (await ledger.Dashboard.GetSummaryAsync(ledger.UserId)).Data!;
        var atRisk = (await ledger.Dashboard.GetBudgetsAtRiskAsync(ledger.UserId)).Data!;

        Assert.Equal(month.TotalBudgeted, summary.Current.Budgeted);
        Assert.Equal(month.Remaining, summary.Current.BudgetRemaining);
        var transport = Assert.Single(atRisk);
        Assert.Equal(month.Lines.Single(l => l.CategoryId == ledger.Transport.Id).PercentUsed, transport.PercentUsed);
    }

    [Fact]
    public async Task GetMonth_RejectsAMidMonthDate()
    {
        var ledger = new TestLedger();
        Assert.Equal(ErrorStatus.ValidationError, (await ledger.Budgets.GetMonthAsync(ledger.UserId, Sep10)).ErrorStatus);
    }

    // ---- set -----------------------------------------------------------------------------

    [Fact]
    public async Task Set_CreatesThenUpdatesTheSameRow()
    {
        var ledger = new TestLedger();

        await ledger.Budgets.SetAsync(ledger.UserId, new SetBudgetRequest { CategoryId = ledger.Food.Id, Month = Sep, Amount = 5_000 });
        var result = await ledger.Budgets.SetAsync(ledger.UserId, new SetBudgetRequest { CategoryId = ledger.Food.Id, Month = Sep, Amount = 7_000 });

        Assert.True(result.IsSuccess);
        var row = Assert.Single(ledger.Context.Budgets);
        Assert.Equal(7_000, row.Amount);
        Assert.Equal(7_000, result.Data!.TotalBudgeted);
    }

    [Fact]
    public async Task Set_RejectsIncomeCategories()
    {
        var ledger = new TestLedger();

        var topLevel = await ledger.Budgets.SetAsync(ledger.UserId, new SetBudgetRequest { CategoryId = ledger.Salary.Id, Month = Sep, Amount = 100 });
        var sub = await ledger.Budgets.SetAsync(ledger.UserId, new SetBudgetRequest { CategoryId = ledger.Bonus.Id, Month = Sep, Amount = 100 });

        Assert.Equal(ErrorStatus.ValidationError, topLevel.ErrorStatus);
        Assert.Equal(ErrorStatus.ValidationError, sub.ErrorStatus);
        Assert.Empty(ledger.Context.Budgets);
    }

    [Fact]
    public async Task Set_RejectsASubCategoryWhoseParentIsNotExpense()
    {
        var ledger = new TestLedger();
        // Inconsistent data: an "Expense" child under an Income parent.
        var odd = new Category { UserId = ledger.UserId, Name = "Odd", Type = CategoryType.Expense, ParentId = ledger.Salary.Id };
        ledger.Context.Categories.Add(odd);
        ledger.Context.SaveChanges();

        var result = await ledger.Budgets.SetAsync(ledger.UserId, new SetBudgetRequest { CategoryId = odd.Id, Month = Sep, Amount = 100 });

        Assert.Equal("Budgets can only be set on expense categories.", result.Message);
        Assert.Empty(ledger.Context.Budgets);
    }

    // ---- copy from previous month --------------------------------------------------------

    [Fact]
    public async Task CopyFromPrevious_CopiesEveryRowIntoAnEmptyMonth()
    {
        var ledger = new TestLedger();
        ledger.Budget(ledger.Food, Aug, 10_000);
        ledger.Budget(ledger.DiningOut, Aug, 3_000);

        var result = await ledger.Budgets.CopyFromPreviousAsync(ledger.UserId, Sep);

        Assert.Equal(2, result.Data);
        var sep = ledger.Context.Budgets.Where(b => b.Month == Sep).ToList();
        Assert.Equal([3_000L, 10_000L], sep.Select(b => b.Amount).Order());
        Assert.True((await ledger.Budgets.HasAnyAsync(ledger.UserId, Sep)).Data);
    }

    [Fact]
    public async Task CopyFromPrevious_IsRefusedWhenTheMonthAlreadyHasRows()
    {
        var ledger = new TestLedger();
        ledger.Budget(ledger.Food, Aug, 10_000);
        ledger.Budget(ledger.Transport, Aug, 2_000);
        ledger.Budget(ledger.Food, Sep, 4_000); // user already started September

        var result = await ledger.Budgets.CopyFromPreviousAsync(ledger.UserId, Sep);

        Assert.Equal(ErrorStatus.Duplicate, result.ErrorStatus);
        var sep = Assert.Single(ledger.Context.Budgets.Where(b => b.Month == Sep));
        Assert.Equal(4_000, sep.Amount); // untouched
    }

    [Fact]
    public async Task HasAny_IsFalseForAnEmptyMonth()
    {
        var ledger = new TestLedger();
        ledger.Budget(ledger.Food, Aug, 10_000);

        Assert.False((await ledger.Budgets.HasAnyAsync(ledger.UserId, Sep)).Data);
        Assert.True((await ledger.Budgets.HasAnyAsync(ledger.UserId, Aug)).Data);
    }
}
