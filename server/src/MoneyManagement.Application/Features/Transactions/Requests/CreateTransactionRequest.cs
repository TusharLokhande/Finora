using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Application.Features.Transactions.Requests;

public class CreateTransactionRequest
{
    public TransactionType Type { get; set; }
    public DateOnly TxnDate { get; set; }
    public long Amount { get; set; }
    public Guid AccountId { get; set; }
    public Guid? ToAccountId { get; set; }
    public Guid? CategoryId { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
}
