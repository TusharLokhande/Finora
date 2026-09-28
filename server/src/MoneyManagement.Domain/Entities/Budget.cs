using MoneyManagement.Domain.Common;

namespace MoneyManagement.Domain.Entities;

public class Budget : BaseEntity, IUserOwned
{
    public Guid UserId { get; set; }
    public Guid CategoryId { get; set; }
    public DateOnly Month { get; set; }
    public long Amount { get; set; }

    public User? User { get; set; }
    public Category? Category { get; set; }
}
