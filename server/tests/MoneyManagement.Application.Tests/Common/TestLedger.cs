using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.EntityFrameworkCore;
using MoneyManagement.Application.Common.Services;
using MoneyManagement.Application.Features.Accounts.Services;
using MoneyManagement.Application.Features.Reports.Services;
using MoneyManagement.Application.Features.Budgets.Services;
using MoneyManagement.Application.Features.Dashboard.Services;
using MoneyManagement.Application.Features.Transactions.Services;
using MoneyManagement.Domain.Entities;
using MoneyManagement.Domain.Enums;
using MoneyManagement.Infrastructure.Export;
using MoneyManagement.Infrastructure.Persistence;
using MoneyManagement.Infrastructure.Persistence.Repository;

namespace MoneyManagement.Application.Tests.Common;

/// <summary>
/// One user on an in-memory database with a bank, a card, Food (+ Groceries, Dining out),
/// Transport, Salary (+ Bonus), plus the real services wired over it. Amounts are in minor units.
/// </summary>
public class TestLedger
{
    public AppDbContext Context { get; }
    public Guid UserId { get; } = Guid.NewGuid();

    public Account Bank { get; }
    public Account Card { get; }
    public Category Food { get; }
    public Category Groceries { get; }
    public Category DiningOut { get; }
    public Category Transport { get; }
    public Category Salary { get; }
    public Category Bonus { get; }

    public TransactionService Transactions { get; }
    public BudgetService Budgets { get; }
    public ReportService Reports { get; }
    public DashboardService Dashboard { get; }

    public TestLedger(DateOnly today = default)
    {
        Context = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

        Context.Users.Add(new User { Id = UserId, Name = "Test", Email = $"{UserId}@example.com", Timezone = "UTC" });

        Bank = new Account { UserId = UserId, Name = "Bank", Type = AccountType.Bank };
        Card = new Account { UserId = UserId, Name = "Card", Type = AccountType.CreditCard, CreditLimit = 100_000, StatementDay = 1, DueDay = 15 };

        Food = new Category { UserId = UserId, Name = "Food", Type = CategoryType.Expense };
        Groceries = new Category { UserId = UserId, Name = "Groceries", Type = CategoryType.Expense, ParentId = Food.Id };
        DiningOut = new Category { UserId = UserId, Name = "Dining out", Type = CategoryType.Expense, ParentId = Food.Id };
        Transport = new Category { UserId = UserId, Name = "Transport", Type = CategoryType.Expense };
        Salary = new Category { UserId = UserId, Name = "Salary", Type = CategoryType.Income };
        Bonus = new Category { UserId = UserId, Name = "Bonus", Type = CategoryType.Income, ParentId = Salary.Id };

        Context.Accounts.AddRange(Bank, Card);
        Context.Categories.AddRange(Food, Groceries, DiningOut, Transport, Salary, Bonus);
        Context.SaveChanges();

        var accounts = new AccountRepository(Context);
        var transactions = new TransactionRepository(Context);
        var categories = new CategoryRepository(Context);
        var unitOfWork = new UnitOfWork(Context);

        Transactions = new TransactionService(transactions, accounts, categories, unitOfWork, new ExcelExportWriter(), NullLogger<TransactionService>.Instance);
        var accountService = new AccountService(accounts, transactions, Transactions, unitOfWork, NullLogger<AccountService>.Instance);

        Budgets = new BudgetService(new BudgetRepository(Context), categories, transactions, unitOfWork);

        var clock = new UserClock(new UserRepository(Context), new FixedTimeProvider(today == default ? DateOnly.FromDateTime(DateTime.UtcNow) : today));
        Reports = new ReportService(transactions, categories, Budgets, clock, new ExcelExportWriter());
        Dashboard = new DashboardService(transactions, Budgets, Reports, accountService, clock);
    }

    public Transaction Expense(long amount, Category category, DateOnly date, string? description = null, Account? account = null)
        => Add(new Transaction { Type = TransactionType.Expense, Amount = amount, CategoryId = category.Id, TxnDate = date, Description = description, AccountId = (account ?? Bank).Id });

    public Transaction Income(long amount, DateOnly date, string? description = null)
        => Add(new Transaction { Type = TransactionType.Income, Amount = amount, CategoryId = Salary.Id, TxnDate = date, Description = description, AccountId = Bank.Id });

    public Transaction Transfer(long amount, DateOnly date)
        => Add(new Transaction { Type = TransactionType.Transfer, Amount = amount, ToAccountId = Card.Id, TxnDate = date, AccountId = Bank.Id });

    public void Budget(Category category, DateOnly month, long amount)
    {
        Context.Budgets.Add(new Budget { UserId = UserId, CategoryId = category.Id, Month = month, Amount = amount });
        Context.SaveChanges();
    }

    private Transaction Add(Transaction transaction)
    {
        transaction.UserId = UserId;
        Context.Transactions.Add(transaction);
        Context.SaveChanges();
        return transaction;
    }

    private sealed class FixedTimeProvider(DateOnly today) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(today.ToDateTime(new TimeOnly(12, 0)), TimeSpan.Zero);
    }
}
