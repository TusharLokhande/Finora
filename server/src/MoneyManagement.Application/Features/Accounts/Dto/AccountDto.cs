using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Application.Features.Accounts.Dto;

public class AccountDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public string? Color { get; set; }
    public int SortOrder { get; set; }
    public bool Active { get; set; }

    /// <summary>Bank/Cash/Wallet only: money available. Null for credit cards.</summary>
    public long? Balance { get; set; }

    /// <summary>Credit cards only: amount owed, always &gt;= 0.</summary>
    public long? Outstanding { get; set; }

    public long? CreditLimit { get; set; }

    /// <summary>Credit cards only: CreditLimit - Outstanding.</summary>
    public long? AvailableCredit { get; set; }

    public short? StatementDay { get; set; }
    public short? DueDay { get; set; }

    /// <summary>Credit cards only: next due date, or the current one if it's overdue and still owed.</summary>
    public DateOnly? NextDueDate { get; set; }
}
