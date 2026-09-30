using MoneyManagement.Application.Common;
using MoneyManagement.Application.Features.Transactions.Requests;
using MoneyManagement.Application.Features.Transactions.Validators;
using MoneyManagement.Application.Tests.Common;
using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Application.Tests.Features.Transactions;

public class TransactionRulesTests
{
    private static readonly DateOnly Sep15 = new(2026, 9, 15);

    private static CreateTransactionRequest Valid(TestLedger l) => new()
    {
        Type = TransactionType.Expense,
        TxnDate = Sep15,
        Amount = 1_000,
        AccountId = l.Bank.Id,
        CategoryId = l.Food.Id,
    };

    // Last column: whether the FluentValidator (run by the API filter) also catches it without the database.
    public static TheoryData<string, Action<CreateTransactionRequest, TestLedger>, string, bool> Invalid => new()
    {
        { "zero amount", (r, _) => r.Amount = 0, "Amount must be greater than zero.", true },
        { "negative amount", (r, _) => r.Amount = -5, "Amount must be greater than zero.", true },
        { "transfer without destination", (r, _) => { r.Type = TransactionType.Transfer; r.CategoryId = null; }, "A transfer needs a destination account.", true },
        { "transfer with category", (r, l) => { r.Type = TransactionType.Transfer; r.ToAccountId = l.Card.Id; }, "Transfers cannot have a category.", true },
        { "transfer to itself", (r, l) => { r.Type = TransactionType.Transfer; r.CategoryId = null; r.ToAccountId = l.Bank.Id; }, "Source and destination accounts must be different.", false },
        { "expense without category", (r, _) => r.CategoryId = null, "A category is required.", true },
        { "expense with destination", (r, l) => r.ToAccountId = l.Card.Id, "Only transfers can have a destination account.", true },
        { "income with expense category", (r, _) => r.Type = TransactionType.Income, "Category type does not match the transaction type.", false },
        { "expense with income category", (r, l) => r.CategoryId = l.Salary.Id, "Category type does not match the transaction type.", false },
    };

    [Theory]
    [MemberData(nameof(Invalid))]
    public async Task Create_RejectsRuleViolations(string _, Action<CreateTransactionRequest, TestLedger> breakIt, string message, bool __)
    {
        var ledger = new TestLedger();
        var request = Valid(ledger);
        breakIt(request, ledger);

        var result = await ledger.Transactions.CreateAsync(ledger.UserId, request);

        Assert.False(result.IsSuccess);
        Assert.Equal(message, result.Message);
        Assert.Empty(ledger.Context.Transactions);
    }

    [Fact]
    public async Task Create_AcceptsValidExpenseIncomeAndTransfer()
    {
        var ledger = new TestLedger();

        var expense = await ledger.Transactions.CreateAsync(ledger.UserId, Valid(ledger));
        var incomeRequest = Valid(ledger);
        incomeRequest.Type = TransactionType.Income;
        incomeRequest.CategoryId = ledger.Salary.Id;
        var income = await ledger.Transactions.CreateAsync(ledger.UserId, incomeRequest);
        var transfer = await ledger.Transactions.CreateAsync(ledger.UserId, new CreateTransactionRequest
        {
            Type = TransactionType.Transfer, TxnDate = Sep15, Amount = 1, AccountId = ledger.Bank.Id, ToAccountId = ledger.Card.Id,
        });

        Assert.True(expense.IsSuccess);
        Assert.True(income.IsSuccess);
        Assert.True(transfer.IsSuccess);
    }

    [Theory]
    [MemberData(nameof(Invalid))]
    public void Validator_RejectsTheShapeRules(string _, Action<CreateTransactionRequest, TestLedger> breakIt, string __, bool validatorCatchesIt)
    {
        if (!validatorCatchesIt)
            return;

        var ledger = new TestLedger();
        var request = Valid(ledger);
        breakIt(request, ledger);

        Assert.False(new CreateTransactionValidator().Validate(request).IsValid);
    }

    [Fact]
    public async Task BulkDelete_SoftDeletesAndBulkRestoreBringsBack()
    {
        var ledger = new TestLedger();
        var a = ledger.Expense(100, ledger.Food, Sep15);
        var b = ledger.Expense(100, ledger.Food, Sep15);
        var untouched = ledger.Expense(100, ledger.Food, Sep15);

        var deleted = await ledger.Transactions.BulkDeleteAsync(ledger.UserId, new() { Ids = [a.Id, b.Id] });

        Assert.Equal(2, deleted.Data);
        Assert.False(a.Active);
        Assert.False(b.Active);
        Assert.True(untouched.Active);

        var restored = await ledger.Transactions.BulkRestoreAsync(ledger.UserId, new() { Ids = [a.Id, b.Id] });

        Assert.Equal(2, restored.Data);
        Assert.True(a.Active);
        Assert.True(b.Active);
    }

    [Fact]
    public async Task BulkDelete_IsAllOrNothingWhenAnIdIsUnknown()
    {
        var ledger = new TestLedger();
        var a = ledger.Expense(100, ledger.Food, Sep15);

        var result = await ledger.Transactions.BulkDeleteAsync(ledger.UserId, new() { Ids = [a.Id, Guid.NewGuid()] });

        Assert.Equal(ErrorStatus.NotFound, result.ErrorStatus);
        Assert.True(a.Active);
    }

    [Fact]
    public async Task BulkDelete_CannotTouchAnotherUsersTransactions()
    {
        var ledger = new TestLedger();
        var mine = ledger.Expense(100, ledger.Food, Sep15);

        var result = await ledger.Transactions.BulkDeleteAsync(Guid.NewGuid(), new() { Ids = [mine.Id] });

        Assert.False(result.IsSuccess);
        Assert.True(mine.Active);
    }

    [Fact]
    public async Task BulkRecategorize_MovesAllToTheNewCategory()
    {
        var ledger = new TestLedger();
        var a = ledger.Expense(100, ledger.Food, Sep15);
        var b = ledger.Expense(100, ledger.Transport, Sep15);

        var result = await ledger.Transactions.BulkRecategorizeAsync(ledger.UserId, new() { Ids = [a.Id, b.Id], CategoryId = ledger.DiningOut.Id });

        Assert.Equal(2, result.Data);
        Assert.Equal(ledger.DiningOut.Id, a.CategoryId);
        Assert.Equal(ledger.DiningOut.Id, b.CategoryId);
    }

    [Fact]
    public async Task BulkRecategorize_RejectsTypeMismatchWithoutChangingAnything()
    {
        var ledger = new TestLedger();
        var expense = ledger.Expense(100, ledger.Food, Sep15);
        var income = ledger.Income(100, Sep15);

        var result = await ledger.Transactions.BulkRecategorizeAsync(ledger.UserId, new() { Ids = [expense.Id, income.Id], CategoryId = ledger.Transport.Id });

        Assert.Equal(ErrorStatus.ValidationError, result.ErrorStatus);
        Assert.Equal(ledger.Food.Id, expense.CategoryId);
        Assert.Equal(ledger.Salary.Id, income.CategoryId);
    }

    [Fact]
    public async Task BulkRecategorize_RejectsTransfers()
    {
        var ledger = new TestLedger();
        var transfer = ledger.Transfer(100, Sep15);

        var result = await ledger.Transactions.BulkRecategorizeAsync(ledger.UserId, new() { Ids = [transfer.Id], CategoryId = ledger.Food.Id });

        Assert.Equal("Transfers cannot have a category.", result.Message);
        Assert.Null(transfer.CategoryId);
    }

    [Fact]
    public void BulkValidator_RequiresIds()
    {
        Assert.False(new BulkTransactionsValidator().Validate(new BulkTransactionsRequest()).IsValid);
        Assert.False(new BulkRecategorizeValidator().Validate(new BulkRecategorizeRequest { Ids = [Guid.NewGuid()] }).IsValid);
    }
}
