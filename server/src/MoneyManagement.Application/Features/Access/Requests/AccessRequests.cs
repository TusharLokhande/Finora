namespace MoneyManagement.Application.Features.Access.Requests;

public record RejectMemberRequest(string? Reason);

public record UpdateAccessSettingsRequest(bool SignupsOpen);
