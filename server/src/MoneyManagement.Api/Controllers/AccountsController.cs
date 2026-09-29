using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyManagement.Api.Common;
using MoneyManagement.Application.Common.Interfaces;
using MoneyManagement.Application.Features.Accounts.Interfaces;
using MoneyManagement.Application.Features.Accounts.Requests;

namespace MoneyManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/accounts")]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly ICurrentUserService _currentUser;

    public AccountsController(IAccountService accountService, ICurrentUserService currentUser)
    {
        _accountService = accountService;
        _currentUser = currentUser;
    }

    private Guid UserId => _currentUser.UserId!.Value;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _accountService.GetAllAsync(UserId, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAccountRequest request, CancellationToken cancellationToken)
    {
        var result = await _accountService.CreateAsync(UserId, request, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAccountRequest request, CancellationToken cancellationToken)
    {
        var result = await _accountService.UpdateAsync(UserId, id, request, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPatch("{id:guid}/archive")]
    public async Task<IActionResult> Archive(Guid id, CancellationToken cancellationToken)
    {
        var result = await _accountService.ArchiveAsync(UserId, id, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPatch("{id:guid}/restore")]
    public async Task<IActionResult> Restore(Guid id, CancellationToken cancellationToken)
    {
        var result = await _accountService.RestoreAsync(UserId, id, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPatch("reorder")]
    public async Task<IActionResult> Reorder([FromBody] ReorderAccountsRequest request, CancellationToken cancellationToken)
    {
        var result = await _accountService.ReorderAsync(UserId, request, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("{id:guid}/pay")]
    public async Task<IActionResult> PayCard(Guid id, [FromBody] PayCardRequest request, CancellationToken cancellationToken)
    {
        var result = await _accountService.PayCardAsync(UserId, id, request, cancellationToken);
        return result.ToActionResult();
    }
}
