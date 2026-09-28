using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Domain.Entities;

public class UserSettings
{
    public Guid UserId { get; set; }
    public Theme Theme { get; set; } = Theme.System;
    public Density Density { get; set; } = Density.Comfortable;
    public Guid? LastUsedAccountId { get; set; }
    public TransactionType? LastUsedType { get; set; }

    public User? User { get; set; }
}
