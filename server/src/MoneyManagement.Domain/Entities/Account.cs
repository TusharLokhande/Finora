using MoneyManagement.Domain.Common;
using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Domain.Entities;

public class Account : BaseEntity, IUserOwned
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public AccountType Type { get; set; }
    public long OpeningBalance { get; set; }
    public long? CreditLimit { get; set; }
    public short? StatementDay { get; set; }
    public short? DueDay { get; set; }
    public string? Color { get; set; }
    public int SortOrder { get; set; }

    public User? User { get; set; }
}
