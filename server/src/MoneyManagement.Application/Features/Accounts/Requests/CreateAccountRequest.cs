using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Application.Features.Accounts.Requests;

public class CreateAccountRequest
{
    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; }

    /// <summary>
    /// Positive amount. For CreditCard this is "amount owed" as entered by the user;
    /// the service converts it to the internally-stored negative opening balance.
    /// </summary>
    public long OpeningBalance { get; set; }

    public long? CreditLimit { get; set; }
    public short? StatementDay { get; set; }
    public short? DueDay { get; set; }
    public string? Color { get; set; }
}
