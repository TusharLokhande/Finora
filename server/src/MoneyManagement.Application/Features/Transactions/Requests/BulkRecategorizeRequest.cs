namespace MoneyManagement.Application.Features.Transactions.Requests;

public class BulkRecategorizeRequest : BulkTransactionsRequest
{
    public Guid CategoryId { get; set; }
}
