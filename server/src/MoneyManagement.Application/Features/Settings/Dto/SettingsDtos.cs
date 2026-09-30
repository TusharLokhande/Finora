using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Application.Features.Settings.Dto;

public record SettingsDto(string Name, string Email, string CurrencyCode, string Timezone, Theme Theme, Density Density);

public record ExportAccountDto(Guid Id, string Name, AccountType Type, long OpeningBalance, long? CreditLimit, short? StatementDay, short? DueDay, string? Color, int SortOrder, bool Active, DateTime CreatedAtUtc);

public record ExportCategoryDto(Guid Id, Guid? ParentId, string Name, CategoryType Type, string? Color, string? Icon, int SortOrder, bool Active, DateTime CreatedAtUtc);

public record ExportTransactionDto(Guid Id, TransactionType Type, DateOnly TxnDate, long Amount, Guid AccountId, Guid? ToAccountId, Guid? CategoryId, string? Description, string? Notes, bool Active, DateTime CreatedAtUtc);

public record ExportBudgetDto(Guid Id, Guid CategoryId, DateOnly Month, long Amount, bool Active, DateTime CreatedAtUtc);

/// <summary>Everything the user owns. Amounts are in minor units of <see cref="CurrencyCode"/>.</summary>
public record UserExportDto(
    DateTime ExportedAtUtc,
    string Name,
    string Email,
    string CurrencyCode,
    string Timezone,
    IReadOnlyList<ExportAccountDto> Accounts,
    IReadOnlyList<ExportCategoryDto> Categories,
    IReadOnlyList<ExportTransactionDto> Transactions,
    IReadOnlyList<ExportBudgetDto> Budgets);
