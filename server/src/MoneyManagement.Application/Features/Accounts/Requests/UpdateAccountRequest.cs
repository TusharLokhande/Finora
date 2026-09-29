namespace MoneyManagement.Application.Features.Accounts.Requests;

public class UpdateAccountRequest
{
    public string Name { get; set; } = string.Empty;

    /// <summary>Same convention as CreateAccountRequest: positive "amount owed" for cards.</summary>
    public long OpeningBalance { get; set; }

    public long? CreditLimit { get; set; }
    public short? StatementDay { get; set; }
    public short? DueDay { get; set; }
    public string? Color { get; set; }
}
