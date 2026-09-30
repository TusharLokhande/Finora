using MoneyManagement.Application.Common;
using MoneyManagement.Application.Common.Dashboard;
using MoneyManagement.Application.Features.Access.Dto;
using MoneyManagement.Application.Features.Access.Requests;

namespace MoneyManagement.Application.Features.Access.Interfaces;

public interface IAccessService
{
    /// <param name="status">Pending, Approved, Rejected, Suspended, or All/null.</param>
    Task<Result<MemberListDto>> GetMembersAsync(string? status, CancellationToken cancellationToken = default);

    Task<Result<MemberDto>> ApproveAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<object?>> RejectAsync(Guid id, RejectMemberRequest request, CancellationToken cancellationToken = default);

    Task<Result<MemberDto>> SuspendAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<MemberDto>> ReactivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<object?>> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<AccessSettingsDto>> GetSettingsAsync(CancellationToken cancellationToken = default);

    Task<Result<AccessSettingsDto>> UpdateSettingsAsync(UpdateAccessSettingsRequest request, CancellationToken cancellationToken = default);

    Task<Result<PageResult<AuditLogDto>>> GetAuditLogAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}
