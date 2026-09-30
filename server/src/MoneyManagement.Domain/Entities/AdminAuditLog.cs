using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Domain.Entities;

/// <summary>Append-only. TargetUserId has no FK on purpose so entries outlive a deleted user; TargetEmail is the snapshot.</summary>
public class AdminAuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AdminUserId { get; set; }
    public AdminAction Action { get; set; }
    public Guid? TargetUserId { get; set; }
    public string? TargetEmail { get; set; }
    public string? Reason { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
