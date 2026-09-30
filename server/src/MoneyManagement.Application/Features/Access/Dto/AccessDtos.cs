using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Application.Features.Access.Dto;

public record MemberDto(Guid Id, string Name, string Email, UserStatus Status, UserRole Role, DateTime CreatedAtUtc);

public record MemberCountsDto(int All, int Pending, int Approved, int Rejected, int Suspended);

public record MemberListDto(IReadOnlyList<MemberDto> Items, MemberCountsDto Counts);

public record AccessSettingsDto(bool SignupsOpen);

public record AuditLogDto(Guid Id, AdminAction Action, string AdminEmail, string? TargetEmail, string? Reason, DateTime CreatedAtUtc);
