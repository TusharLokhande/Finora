using MoneyManagement.Domain.Common;
using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Domain.Entities;

public class Transaction : BaseEntity, IUserOwned
{
    public Guid UserId { get; set; }
    public TransactionType Type { get; set; }
    public DateOnly TxnDate { get; set; }
    public long Amount { get; set; }
    public Guid AccountId { get; set; }
    public Guid? ToAccountId { get; set; }
    public Guid? CategoryId { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }

    public User? User { get; set; }
    public Account? Account { get; set; }
    public Account? ToAccount { get; set; }
    public Category? Category { get; set; }
}
