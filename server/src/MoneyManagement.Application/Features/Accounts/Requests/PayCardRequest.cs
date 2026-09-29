namespace MoneyManagement.Application.Features.Accounts.Requests;

public class PayCardRequest
{
    public Guid SourceAccountId { get; set; }
    public long Amount { get; set; }
    public DateOnly Date { get; set; }
    public string? Note { get; set; }
}
