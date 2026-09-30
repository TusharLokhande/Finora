using MoneyManagement.Application.Common;
using MoneyManagement.Application.Common.Dashboard;
using MoneyManagement.Application.Common.Interfaces;
using MoneyManagement.Application.Features.Access.Dto;
using MoneyManagement.Application.Features.Access.Interfaces;
using MoneyManagement.Application.Features.Access.Requests;
using MoneyManagement.Application.Features.Auth.Interfaces;
using MoneyManagement.Application.Interfaces.UnitOfWork;
using MoneyManagement.Domain.Entities;
using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Application.Features.Access.Services;

public class AccessService : IAccessService
{
    private readonly IUserRepository _users;
    private readonly IAdminAuditLogRepository _audit;
    private readonly IAppSettingRepository _settings;
    private readonly ICurrentUserService _currentUser;
    private readonly IUnitOfWork _unitOfWork;

    public AccessService(
        IUserRepository users,
        IAdminAuditLogRepository audit,
        IAppSettingRepository settings,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork)
    {
        _users = users;
        _audit = audit;
        _settings = settings;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<MemberListDto>> GetMembersAsync(string? status, CancellationToken cancellationToken = default)
    {
        UserStatus? filter = null;
        if (!string.IsNullOrEmpty(status) && !status.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            if (!Enum.TryParse<UserStatus>(status, ignoreCase: true, out var parsed))
                return Result<MemberListDto>.Failure("Unknown status filter.", ErrorStatus.ValidationError);
            filter = parsed;
        }

        var users = await _users.ListAsync(filter, cancellationToken);
        var counts = await _users.CountByStatusAsync(cancellationToken);
        int Count(UserStatus s) => counts.GetValueOrDefault(s);

        var dto = new MemberListDto(
            users.Select(ToDto).ToList(),
            new MemberCountsDto(counts.Values.Sum(), Count(UserStatus.Pending), Count(UserStatus.Approved), Count(UserStatus.Rejected), Count(UserStatus.Suspended)));

        return Result<MemberListDto>.Success(dto);
    }

    public Task<Result<MemberDto>> ApproveAsync(Guid id, CancellationToken cancellationToken = default)
        => TransitionAsync(id, UserStatus.Pending, UserStatus.Approved, AdminAction.Approve, cancellationToken);

    public Task<Result<MemberDto>> SuspendAsync(Guid id, CancellationToken cancellationToken = default)
        => TransitionAsync(id, UserStatus.Approved, UserStatus.Suspended, AdminAction.Suspend, cancellationToken);

    public Task<Result<MemberDto>> ReactivateAsync(Guid id, CancellationToken cancellationToken = default)
        => TransitionAsync(id, UserStatus.Suspended, UserStatus.Approved, AdminAction.Reactivate, cancellationToken);

    // A Pending user has no data yet, so a reject is a hard delete; the audit row keeps the email snapshot.
    public Task<Result<object?>> RejectAsync(Guid id, RejectMemberRequest request, CancellationToken cancellationToken = default)
        => RemoveAsync(id, [UserStatus.Pending], AdminAction.Reject, request.Reason?.Trim(), cancellationToken);

    public Task<Result<object?>> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        => RemoveAsync(id, [UserStatus.Approved, UserStatus.Suspended], AdminAction.DeleteUser, null, cancellationToken);

    public async Task<Result<AccessSettingsDto>> GetSettingsAsync(CancellationToken cancellationToken = default)
        => Result<AccessSettingsDto>.Success(new AccessSettingsDto(await _settings.GetSignupsOpenAsync(cancellationToken)));

    public async Task<Result<AccessSettingsDto>> UpdateSettingsAsync(UpdateAccessSettingsRequest request, CancellationToken cancellationToken = default)
    {
        if (await _settings.GetSignupsOpenAsync(cancellationToken) != request.SignupsOpen)
        {
            await _settings.SetSignupsOpenAsync(request.SignupsOpen, cancellationToken);
            await _audit.AddAsync(NewEntry(AdminAction.ToggleSignups, null, request.SignupsOpen ? "Accepting new requests" : "Not accepting new requests"), cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result<AccessSettingsDto>.Success(new AccessSettingsDto(request.SignupsOpen));
    }

    public async Task<Result<PageResult<AuditLogDto>>> GetAuditLogAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var (items, total) = await _audit.ListAsync((page - 1) * pageSize, pageSize, cancellationToken);
        return Result<PageResult<AuditLogDto>>.Success(new PageResult<AuditLogDto> { Items = items, TotalCount = total, Page = page, PageSize = pageSize });
    }

    private async Task<Result<MemberDto>> TransitionAsync(Guid id, UserStatus from, UserStatus to, AdminAction action, CancellationToken cancellationToken)
    {
        var found = await FindTargetAsync(id, [from], cancellationToken);
        if (!found.IsSuccess)
            return Result<MemberDto>.Failure(found.Message, found.ErrorStatus);

        var user = found.Data!;
        user.Status = to;
        user.UpdatedAtUtc = DateTime.UtcNow;
        _users.Update(user);
        await _audit.AddAsync(NewEntry(action, user, null), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<MemberDto>.Success(ToDto(user));
    }

    private async Task<Result<object?>> RemoveAsync(Guid id, UserStatus[] allowed, AdminAction action, string? reason, CancellationToken cancellationToken)
    {
        var found = await FindTargetAsync(id, allowed, cancellationToken);
        if (!found.IsSuccess)
            return Result<object?>.Failure(found.Message, found.ErrorStatus);

        var user = found.Data!;
        await _audit.AddAsync(NewEntry(action, user, string.IsNullOrEmpty(reason) ? null : reason), cancellationToken);
        await _users.RemoveWithDataAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<object?>.Success(null);
    }

    private async Task<Result<User>> FindTargetAsync(Guid id, UserStatus[] allowed, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(id, cancellationToken);
        if (user is null)
            return Result<User>.Failure("Member not found.", ErrorStatus.NotFound);
        if (user.Role == UserRole.Admin)
            return Result<User>.Failure("Admin accounts can't be changed from here.", ErrorStatus.Forbidden);
        if (!allowed.Contains(user.Status))
            return Result<User>.Failure($"This member is {user.Status.ToString().ToLowerInvariant()}, so that action doesn't apply.", ErrorStatus.Duplicate);

        return Result<User>.Success(user);
    }

    private AdminAuditLog NewEntry(AdminAction action, User? target, string? reason) => new()
    {
        AdminUserId = _currentUser.UserId!.Value,
        Action = action,
        TargetUserId = target?.Id,
        TargetEmail = target?.Email,
        Reason = reason,
    };

    private static MemberDto ToDto(User u) => new(u.Id, u.Name, u.Email, u.Status, u.Role, u.CreatedAtUtc);
}
