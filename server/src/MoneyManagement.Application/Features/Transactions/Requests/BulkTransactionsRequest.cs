namespace MoneyManagement.Application.Features.Transactions.Requests;

public class BulkTransactionsRequest
{
    public List<Guid> Ids { get; set; } = [];
}
