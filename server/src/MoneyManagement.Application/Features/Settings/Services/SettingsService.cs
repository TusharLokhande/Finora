using MoneyManagement.Application.Common;
using MoneyManagement.Application.Features.Accounts.Interfaces;
using MoneyManagement.Application.Features.Auth.Interfaces;
using MoneyManagement.Application.Features.Budgets.Interfaces;
using MoneyManagement.Application.Features.Categories.Interfaces;
using MoneyManagement.Application.Features.Settings.Dto;
using MoneyManagement.Application.Features.Settings.Interfaces;
using MoneyManagement.Application.Features.Settings.Requests;
using MoneyManagement.Application.Features.Transactions.Interfaces;
using MoneyManagement.Application.Interfaces.UnitOfWork;
using MoneyManagement.Domain.Entities;
using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Application.Features.Settings.Services;

public class SettingsService : ISettingsService
{
    /// <summary>Must match DELETE_CONFIRMATION in the client's settings constants.</summary>
    public const string DeleteConfirmation = "DELETE MY ACCOUNT";

    private readonly IUserRepository _users;
    private readonly IUserSettingsRepository _settings;
    private readonly IAccountRepository _accounts;
    private readonly ICategoryRepository _categories;
    private readonly ITransactionRepository _transactions;
    private readonly IBudgetRepository _budgets;
    private readonly IUnitOfWork _unitOfWork;

    public SettingsService(
        IUserRepository users,
        IUserSettingsRepository settings,
        IAccountRepository accounts,
        ICategoryRepository categories,
        ITransactionRepository transactions,
        IBudgetRepository budgets,
        IUnitOfWork unitOfWork)
    {
        _users = users;
        _settings = settings;
        _accounts = accounts;
        _categories = categories;
        _transactions = transactions;
        _budgets = budgets;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SettingsDto>> GetAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return Result<SettingsDto>.Failure("User not found.", ErrorStatus.NotFound);

        return Result<SettingsDto>.Success(ToDto(user, await _settings.GetAsync(userId, cancellationToken)));
    }

    public async Task<Result<SettingsDto>> UpdateProfileAsync(Guid userId, UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return Result<SettingsDto>.Failure("User not found.", ErrorStatus.NotFound);

        user.Name = request.Name.Trim();
        user.UpdatedAtUtc = DateTime.UtcNow;
        _users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<SettingsDto>.Success(ToDto(user, await _settings.GetAsync(userId, cancellationToken)));
    }

    public async Task<Result<SettingsDto>> UpdatePreferencesAsync(Guid userId, UpdatePreferencesRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Extra is { Count: > 0 })
        {
            // Currency is locked after onboarding: say so loudly rather than ignoring the field.
            var errors = request.Extra.Keys.ToDictionary(
                key => key,
                key => new[] { key.Replace("_", "").Equals("currencyCode", StringComparison.OrdinalIgnoreCase) ? "Currency can't be changed." : "Unknown field." });
            return Result<SettingsDto>.ValidationFailure(errors);
        }

        if (!TimeZoneInfo.TryFindSystemTimeZoneById(request.Timezone ?? string.Empty, out _))
            return Result<SettingsDto>.ValidationFailure(new() { ["timezone"] = ["Unknown timezone."] });

        var user = await _users.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return Result<SettingsDto>.Failure("User not found.", ErrorStatus.NotFound);

        var settings = await _settings.GetAsync(userId, cancellationToken);
        if (settings is null)
        {
            settings = new UserSettings { UserId = userId };
            await _settings.AddAsync(settings, cancellationToken);
        }

        user.Timezone = request.Timezone!;
        user.UpdatedAtUtc = DateTime.UtcNow;
        settings.Theme = request.Theme;
        settings.Density = request.Density;
        _users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<SettingsDto>.Success(ToDto(user, settings));
    }

    public async Task<Result<UserExportDto>> ExportAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return Result<UserExportDto>.Failure("User not found.", ErrorStatus.NotFound);

        var accounts = await _accounts.GetAllForUserAsync(userId, cancellationToken);
        var categories = await _categories.GetAllForUserAsync(userId, cancellationToken);
        var transactions = await _transactions.GetAllForUserAsync(userId, cancellationToken);
        var budgets = await _budgets.GetAllForUserAsync(userId, cancellationToken);

        return Result<UserExportDto>.Success(new UserExportDto(
            DateTime.UtcNow,
            user.Name,
            user.Email,
            user.CurrencyCode,
            user.Timezone,
            accounts.Select(a => new ExportAccountDto(a.Id, a.Name, a.Type, a.OpeningBalance, a.CreditLimit, a.StatementDay, a.DueDay, a.Color, a.SortOrder, a.Active, a.CreatedAtUtc)).ToList(),
            categories.Select(c => new ExportCategoryDto(c.Id, c.ParentId, c.Name, c.Type, c.Color, c.Icon, c.SortOrder, c.Active, c.CreatedAtUtc)).ToList(),
            transactions.Select(t => new ExportTransactionDto(t.Id, t.Type, t.TxnDate, t.Amount, t.AccountId, t.ToAccountId, t.CategoryId, t.Description, t.Notes, t.Active, t.CreatedAtUtc)).ToList(),
            budgets.Select(b => new ExportBudgetDto(b.Id, b.CategoryId, b.Month, b.Amount, b.Active, b.CreatedAtUtc)).ToList()));
    }

    public async Task<Result<object?>> DeleteAccountAsync(Guid userId, DeleteAccountRequest request, CancellationToken cancellationToken = default)
    {
        if (!string.Equals(request.Confirmation, DeleteConfirmation, StringComparison.Ordinal))
            return Result<object?>.ValidationFailure(new() { ["confirmation"] = [$"Type {DeleteConfirmation} to confirm."] });

        var user = await _users.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return Result<object?>.Failure("User not found.", ErrorStatus.NotFound);

        // Admin actions are recorded against the admin's id (FK), so an admin can't remove themselves this way.
        if (user.Role == UserRole.Admin)
            return Result<object?>.Failure("Admin accounts can't be deleted.", ErrorStatus.Forbidden);

        await _users.RemoveWithDataAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<object?>.Success(null);
    }

    private static SettingsDto ToDto(User user, UserSettings? settings)
        => new(user.Name, user.Email, user.CurrencyCode, user.Timezone, settings?.Theme ?? Theme.System, settings?.Density ?? Density.Comfortable);
}
