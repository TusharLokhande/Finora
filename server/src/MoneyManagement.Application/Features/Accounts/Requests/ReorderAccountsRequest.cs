namespace MoneyManagement.Application.Features.Accounts.Requests;

public class ReorderAccountsRequest
{
    /// <summary>Account ids in the desired display order. SortOrder is assigned from index.</summary>
    public List<Guid> AccountIds { get; set; } = [];
}
