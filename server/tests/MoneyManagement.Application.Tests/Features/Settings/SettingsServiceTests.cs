using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MoneyManagement.Application.Common;
using MoneyManagement.Application.Features.Settings.Requests;
using MoneyManagement.Application.Features.Settings.Services;
using MoneyManagement.Domain.Entities;
using MoneyManagement.Domain.Enums;
using MoneyManagement.Infrastructure.Persistence;
using MoneyManagement.Infrastructure.Persistence.Repository;

namespace MoneyManagement.Application.Tests.Features.Settings;

public class SettingsServiceTests
{
    private readonly AppDbContext _context = new(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
    private readonly SettingsService _service;

    public SettingsServiceTests()
    {
        _service = new SettingsService(
            new UserRepository(_context), new UserSettingsRepository(_context), new AccountRepository(_context),
            new CategoryRepository(_context), new TransactionRepository(_context), new BudgetRepository(_context), new UnitOfWork(_context));
    }

    /// <summary>A user with one row in every user-owned table.</summary>
    private User SeedUser(string email)
    {
        var user = new User { Name = email, Email = email };
        var account = new Account { UserId = user.Id, Name = "Bank", Type = AccountType.Bank };
        var category = new Category { UserId = user.Id, Name = "Food", Type = CategoryType.Expense };
        _context.Users.Add(user);
        _context.Accounts.Add(account);
        _context.Categories.Add(category);
        _context.Transactions.Add(new Transaction { UserId = user.Id, AccountId = account.Id, CategoryId = category.Id, Type = TransactionType.Expense, Amount = 100 });
        _context.Budgets.Add(new Budget { UserId = user.Id, CategoryId = category.Id, Month = new DateOnly(2026, 9, 1), Amount = 500 });
        _context.UserSettings.Add(new UserSettings { UserId = user.Id });
        _context.RefreshTokens.Add(new RefreshToken { UserId = user.Id, TokenHash = Guid.NewGuid().ToString("N"), ExpiresAtUtc = DateTime.UtcNow.AddDays(1) });
        _context.SaveChanges();
        return user;
    }

    private int RowsOwnedBy(Guid userId) =>
        _context.Users.Count(u => u.Id == userId) + _context.Accounts.Count(a => a.UserId == userId)
        + _context.Categories.Count(c => c.UserId == userId) + _context.Transactions.Count(t => t.UserId == userId)
        + _context.Budgets.Count(b => b.UserId == userId) + _context.UserSettings.Count(s => s.UserId == userId)
        + _context.RefreshTokens.Count(r => r.UserId == userId);

    [Fact]
    public async Task Preferences_RejectACurrencyCodeInTheBody()
    {
        var user = SeedUser("a@example.com");
        var request = JsonSerializer.Deserialize<UpdatePreferencesRequest>(
            """{"timezone":"UTC","theme":"Dark","density":"Compact","currency_code":"USD"}""",
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true, Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() } })!;

        var result = await _service.UpdatePreferencesAsync(user.Id, request);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorStatus.ValidationError, result.ErrorStatus);
        Assert.Contains("currency_code", result.Errors!.Keys);
        Assert.Equal("INR", _context.Users.Single(u => u.Id == user.Id).CurrencyCode);
        Assert.Equal(Theme.System, _context.UserSettings.Single(s => s.UserId == user.Id).Theme);
    }

    [Fact]
    public async Task Preferences_SaveTimezoneThemeAndDensity_AndCreateMissingSettings()
    {
        var user = new User { Name = "n", Email = "n@example.com" };
        _context.Users.Add(user);
        _context.SaveChanges();

        var result = await _service.UpdatePreferencesAsync(user.Id, new UpdatePreferencesRequest { Timezone = "UTC", Theme = Theme.Dark, Density = Density.Compact });

        Assert.True(result.IsSuccess);
        Assert.Equal("UTC", _context.Users.Single().Timezone);
        Assert.Equal((Theme.Dark, Density.Compact), (_context.UserSettings.Single().Theme, _context.UserSettings.Single().Density));
    }

    [Fact]
    public async Task DeleteAccount_WithWrongConfirmation_DeletesNothing()
    {
        var user = SeedUser("a@example.com");
        var before = RowsOwnedBy(user.Id);

        var result = await _service.DeleteAccountAsync(user.Id, new DeleteAccountRequest("delete my account"));

        Assert.False(result.IsSuccess);
        Assert.Equal(before, RowsOwnedBy(user.Id));
    }

    [Fact]
    public async Task DeleteAccount_WithMatchingConfirmation_RemovesOnlyThatUsersRows()
    {
        var doomed = SeedUser("a@example.com");
        var other = SeedUser("b@example.com");
        var otherBefore = RowsOwnedBy(other.Id);

        var result = await _service.DeleteAccountAsync(doomed.Id, new DeleteAccountRequest(SettingsService.DeleteConfirmation));

        Assert.True(result.IsSuccess);
        Assert.Equal(0, RowsOwnedBy(doomed.Id));
        Assert.Equal(otherBefore, RowsOwnedBy(other.Id));
    }

    [Fact]
    public async Task DeleteAccount_IsRefusedForAdmins()
    {
        var admin = SeedUser("admin@example.com");
        admin.Role = UserRole.Admin;
        _context.SaveChanges();

        var result = await _service.DeleteAccountAsync(admin.Id, new DeleteAccountRequest(SettingsService.DeleteConfirmation));

        Assert.False(result.IsSuccess);
        Assert.Equal(1, _context.Users.Count(u => u.Id == admin.Id));
    }

    [Fact]
    public async Task Export_CoversEveryUserOwnedTable_OnlyForThatUser()
    {
        var user = SeedUser("a@example.com");
        SeedUser("b@example.com");

        var export = (await _service.ExportAsync(user.Id)).Data!;

        Assert.Equal("a@example.com", export.Email);
        Assert.Single(export.Accounts);
        Assert.Single(export.Categories);
        Assert.Single(export.Transactions);
        Assert.Single(export.Budgets);
    }
}
