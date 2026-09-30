using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyManagement.Api.Common;
using MoneyManagement.Application.Features.Access.Interfaces;
using MoneyManagement.Application.Features.Access.Requests;

namespace MoneyManagement.Api.Controllers;

// Admin-only is enforced for the whole /api/access prefix by UserStatusMiddleware.
[ApiController]
[Authorize]
[Route("api/access")]
public class AccessController : ControllerBase
{
    private readonly IAccessService _accessService;

    public AccessController(IAccessService accessService)
    {
        _accessService = accessService;
    }

    [HttpGet("members")]
    public async Task<IActionResult> GetMembers([FromQuery] string? status, CancellationToken cancellationToken)
        => (await _accessService.GetMembersAsync(status, cancellationToken)).ToActionResult();

    [HttpPost("members/{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
        => (await _accessService.ApproveAsync(id, cancellationToken)).ToActionResult();

    [HttpPost("members/{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectMemberRequest? request, CancellationToken cancellationToken)
        => (await _accessService.RejectAsync(id, request ?? new RejectMemberRequest(null), cancellationToken)).ToActionResult();

    [HttpPost("members/{id:guid}/suspend")]
    public async Task<IActionResult> Suspend(Guid id, CancellationToken cancellationToken)
        => (await _accessService.SuspendAsync(id, cancellationToken)).ToActionResult();

    [HttpPost("members/{id:guid}/reactivate")]
    public async Task<IActionResult> Reactivate(Guid id, CancellationToken cancellationToken)
        => (await _accessService.ReactivateAsync(id, cancellationToken)).ToActionResult();

    [HttpDelete("members/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        => (await _accessService.DeleteAsync(id, cancellationToken)).ToActionResult();

    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings(CancellationToken cancellationToken)
        => (await _accessService.GetSettingsAsync(cancellationToken)).ToActionResult();

    [HttpPut("settings")]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdateAccessSettingsRequest request, CancellationToken cancellationToken)
        => (await _accessService.UpdateSettingsAsync(request, cancellationToken)).ToActionResult();

    [HttpGet("audit-log")]
    public async Task<IActionResult> GetAuditLog([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
        => (await _accessService.GetAuditLogAsync(page, pageSize, cancellationToken)).ToActionResult();
}
