using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Domain.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    public UserStatus Status { get; set; } = UserStatus.Approved;
    public string? RejectionReason { get; set; }
    public string CurrencyCode { get; set; } = "INR";
    public string Timezone { get; set; } = "Asia/Kolkata";
    public DateTime? EmailVerifiedAtUtc { get; set; }
    public DateTime? LastLoginAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }

    public UserSettings? Settings { get; set; }
}
