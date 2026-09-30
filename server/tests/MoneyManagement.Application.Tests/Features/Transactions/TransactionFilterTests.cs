using MoneyManagement.Application.Common.Dashboard;
using MoneyManagement.Application.Features.Transactions.Requests;
using MoneyManagement.Application.Tests.Common;
using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Application.Tests.Features.Transactions;

public class TransactionFilterTests
{
    private static readonly DateOnly Sep1 = new(2026, 9, 1);
    private static readonly DateOnly Sep15 = new(2026, 9, 15);
    private static readonly DateOnly Sep30 = new(2026, 9, 30);
    private static readonly DateOnly Aug31 = new(2026, 8, 31);
    private static readonly DateOnly Oct1 = new(2026, 10, 1);

    private static async Task<List<string?>> Search(TestLedger ledger, TransactionFilterRequest filter)
    {
        var result = await ledger.Transactions.SearchAsync(ledger.UserId, new PageRequest<TransactionFilterRequest>
        {
            Page = 1,
            PageSize = 100,
            CustomFilter = filter,
        });
        Assert.True(result.IsSuccess);
        return result.Data!.Items.Select(t => t.Description).Order().ToList();
    }

    [Fact]
    public async Task Category_MatchesItselfAndItsSubCategories()
    {
        var ledger = new TestLedger();
        ledger.Expense(100, ledger.Food, Sep15, "food");
        ledger.Expense(100, ledger.Groceries, Sep15, "groceries");
        ledger.Expense(100, ledger.DiningOut, Sep15, "dining");
        ledger.Expense(100, ledger.Transport, Sep15, "taxi");
        ledger.Income(100, Sep15, "salary");
        ledger.Transfer(100, Sep15);

        Assert.Equal(["dining", "food", "groceries"], await Search(ledger, new() { CategoryIds = [ledger.Food.Id] }));
    }

    [Fact]
    public async Task SubCategory_MatchesOnlyItself()
    {
        var ledger = new TestLedger();
        ledger.Expense(100, ledger.Food, Sep15, "food");
        ledger.Expense(100, ledger.Groceries, Sep15, "groceries");
        ledger.Expense(100, ledger.DiningOut, Sep15, "dining");

        Assert.Equal(["groceries"], await Search(ledger, new() { CategoryIds = [ledger.Groceries.Id] }));
    }

    [Fact]
    public async Task MultipleCategories_AreOred()
    {
        var ledger = new TestLedger();
        ledger.Expense(100, ledger.Groceries, Sep15, "groceries");
        ledger.Expense(100, ledger.Transport, Sep15, "taxi");
        ledger.Income(100, Sep15, "salary");

        Assert.Equal(["groceries", "salary"], await Search(ledger, new() { CategoryIds = [ledger.Food.Id, ledger.Salary.Id] }));
    }

    [Fact]
    public async Task DateRange_IsInclusiveOnBothEnds()
    {
        var ledger = new TestLedger();
        ledger.Expense(100, ledger.Food, Aug31, "aug31");
        ledger.Expense(100, ledger.Food, Sep1, "sep1");
        ledger.Expense(100, ledger.Food, Sep30, "sep30");
        ledger.Expense(100, ledger.Food, Oct1, "oct1");

        Assert.Equal(["sep1", "sep30"], await Search(ledger, new() { From = Sep1, To = Sep30 }));
        Assert.Equal(["oct1", "sep1", "sep30"], await Search(ledger, new() { From = Sep1 }));
        Assert.Equal(["aug31"], await Search(ledger, new() { To = Aug31 }));
    }

    [Fact]
    public async Task CategoryAndDateRange_Combine()
    {
        var ledger = new TestLedger();
        ledger.Expense(100, ledger.Groceries, Aug31, "old groceries");
        ledger.Expense(100, ledger.Groceries, Sep15, "groceries");
        ledger.Expense(100, ledger.Transport, Sep15, "taxi");

        Assert.Equal(["groceries"], await Search(ledger, new() { CategoryIds = [ledger.Food.Id], From = Sep1, To = Sep30 }));
    }

    [Fact]
    public async Task Account_MatchesEitherSideOfATransfer()
    {
        var ledger = new TestLedger();
        ledger.Expense(100, ledger.Food, Sep15, "on bank");
        ledger.Expense(100, ledger.Food, Sep15, "on card", ledger.Card);
        ledger.Transfer(100, Sep15); // bank -> card, no description

        Assert.Equal([null, "on card"], await Search(ledger, new() { AccountIds = [ledger.Card.Id] }));
    }

    [Fact]
    public async Task TypeAmountAndSearch_Combine()
    {
        var ledger = new TestLedger();
        ledger.Expense(500, ledger.Food, Sep15, "Swiggy lunch");
        ledger.Expense(5_000, ledger.Food, Sep15, "Swiggy party");
        ledger.Expense(50_000, ledger.Food, Sep15, "Swiggy catering");
        ledger.Income(5_000, Sep15, "Swiggy refund");

        var found = await Search(ledger, new()
        {
            Type = TransactionType.Expense,
            MinAmount = 1_000,
            MaxAmount = 10_000,
            Search = "swiggy",
        });

        Assert.Equal(["Swiggy party"], found);
    }

    [Fact]
    public async Task SoftDeletedTransactions_AreExcluded()
    {
        var ledger = new TestLedger();
        var deleted = ledger.Expense(100, ledger.Food, Sep15, "deleted");
        ledger.Expense(100, ledger.Food, Sep15, "kept");

        await ledger.Transactions.DeleteAsync(ledger.UserId, deleted.Id);

        Assert.Equal(["kept"], await Search(ledger, new()));
    }

    [Fact]
    public async Task Recent_ReturnsNewestFirstUpToTheLimit()
    {
        var ledger = new TestLedger();
        for (var day = 1; day <= 10; day++)
            ledger.Expense(100, ledger.Food, new DateOnly(2026, 9, day), $"d{day:00}");

        var recent = await ledger.Transactions.GetRecentAsync(ledger.UserId, 3);

        Assert.Equal(["d10", "d09", "d08"], recent.Data!.Select(t => t.Description));
    }

    [Fact]
    public async Task Suggestions_AreDistinctAndCarryTheLatestCategory()
    {
        var ledger = new TestLedger();
        ledger.Expense(100, ledger.Groceries, new DateOnly(2026, 9, 1), "Swiggy");
        ledger.Expense(100, ledger.DiningOut, new DateOnly(2026, 9, 20), "swiggy");
        ledger.Expense(100, ledger.Transport, new DateOnly(2026, 9, 10), "Uber");

        var suggestions = (await ledger.Transactions.GetDescriptionSuggestionsAsync(ledger.UserId, "sw")).Data!;

        var only = Assert.Single(suggestions);
        Assert.Equal("swiggy", only.Description);
        Assert.Equal(ledger.DiningOut.Id, only.CategoryId);
    }

    [Fact]
    public async Task Export_WritesEveryMatchingRow()
    {
        var ledger = new TestLedger();
        ledger.Expense(100, ledger.Groceries, Sep15, "a");
        ledger.Expense(100, ledger.Transport, Sep15, "b");
        ledger.Income(100, Sep15, "c");

        using var filtered = new MemoryStream();
        var some = await ledger.Transactions.ExportAsync(ledger.UserId, new() { CategoryIds = [ledger.Food.Id] }, filtered);
        using var all = new MemoryStream();
        var everything = await ledger.Transactions.ExportAsync(ledger.UserId, null, all);

        Assert.Equal(1, some.Data);
        Assert.Equal(3, everything.Data);
        Assert.True(all.Length > 0);
    }
}
