using Microsoft.EntityFrameworkCore;
using MoneyManagement.Application.Features.Accounts.Requests;
using MoneyManagement.Application.Features.Accounts.Services;
using MoneyManagement.Application.Features.Transactions.Services;
using MoneyManagement.Domain.Entities;
using MoneyManagement.Domain.Enums;
using MoneyManagement.Infrastructure.Export;
using MoneyManagement.Infrastructure.Persistence;
using MoneyManagement.Infrastructure.Persistence.Repository;

namespace MoneyManagement.Application.Tests.Features.Accounts;

public class AccountServicePayCardTests
{
    private static AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private static (AppDbContext Context, AccountService AccountService, Guid UserId, Account Bank, Account Card) SetUp()
    {
        var context = CreateContext();
        var userId = Guid.NewGuid();

        context.Users.Add(new User { Id = userId, Name = "Test User", Email = "test@example.com" });

        var bank = new Account
        {
            UserId = userId,
            Name = "Checking",
            Type = AccountType.Bank,
            OpeningBalance = 100_000, // ₹1,000.00 in paise
        };

        var card = new Account
        {
            UserId = userId,
            Name = "Visa",
            Type = AccountType.CreditCard,
            OpeningBalance = -20_000, // owed ₹200.00
            CreditLimit = 150_000,
            StatementDay = 1,
            DueDay = 15,
        };

        context.Accounts.AddRange(bank, card);
        context.SaveChanges();

        var accountRepository = new AccountRepository(context);
        var transactionRepository = new TransactionRepository(context);
        var categoryRepository = new CategoryRepository(context);
        var unitOfWork = new UnitOfWork(context);

        var transactionService = new TransactionService(transactionRepository, accountRepository, categoryRepository, unitOfWork, new ExcelExportWriter());
        var accountService = new AccountService(accountRepository, transactionRepository, transactionService, unitOfWork);

        return (context, accountService, userId, bank, card);
    }

    [Fact]
    public async Task PayCardAsync_CreatesTransferTransaction()
    {
        var (context, accountService, userId, bank, card) = SetUp();

        var result = await accountService.PayCardAsync(userId, card.Id, new PayCardRequest
        {
            SourceAccountId = bank.Id,
            Amount = 5_000,
            Date = new DateOnly(2026, 9, 20),
            Note = "Card bill",
        });

        Assert.True(result.IsSuccess);

        var transaction = Assert.Single(context.Transactions);
        Assert.Equal(TransactionType.Transfer, transaction.Type);
        Assert.Equal(bank.Id, transaction.AccountId);
        Assert.Equal(card.Id, transaction.ToAccountId);
        Assert.Equal(5_000, transaction.Amount);
        Assert.Null(transaction.CategoryId);
        Assert.Equal("Card bill", transaction.Notes);
    }

    [Fact]
    public async Task PayCardAsync_MovesBalancesOnBothAccounts()
    {
        var (context, accountService, userId, bank, card) = SetUp();

        await accountService.PayCardAsync(userId, card.Id, new PayCardRequest
        {
            SourceAccountId = bank.Id,
            Amount = 5_000,
            Date = new DateOnly(2026, 9, 20),
        });

        var all = await accountService.GetAllAsync(userId);
        var bankDto = all.Data!.Single(a => a.Id == bank.Id);
        var cardDto = all.Data!.Single(a => a.Id == card.Id);

        // Bank: 100_000 opening - 5_000 paid out.
        Assert.Equal(95_000, bankDto.Balance);

        // Card: owed 20_000, paid 5_000 -> owes 15_000.
        Assert.Equal(15_000, cardDto.Outstanding);
        Assert.Equal(150_000 - 15_000, cardDto.AvailableCredit);
    }

    [Fact]
    public async Task PayCardAsync_PartialPaymentsAccumulateCorrectly()
    {
        var (context, accountService, userId, bank, card) = SetUp();

        await accountService.PayCardAsync(userId, card.Id, new PayCardRequest { SourceAccountId = bank.Id, Amount = 5_000, Date = new DateOnly(2026, 9, 10) });
        await accountService.PayCardAsync(userId, card.Id, new PayCardRequest { SourceAccountId = bank.Id, Amount = 15_000, Date = new DateOnly(2026, 9, 20) });

        var all = await accountService.GetAllAsync(userId);
        var cardDto = all.Data!.Single(a => a.Id == card.Id);

        // Fully paid off: 20_000 owed - 20_000 paid = 0.
        Assert.Equal(0, cardDto.Outstanding);
        Assert.Equal(2, context.Transactions.Count());
    }

    [Fact]
    public async Task PayCardAsync_RejectsPayingANonCreditCardAccount()
    {
        var (context, accountService, userId, bank, card) = SetUp();

        var result = await accountService.PayCardAsync(userId, bank.Id, new PayCardRequest
        {
            SourceAccountId = card.Id,
            Amount = 1_000,
            Date = new DateOnly(2026, 9, 20),
        });

        Assert.False(result.IsSuccess);
        Assert.Empty(context.Transactions);
    }
}
