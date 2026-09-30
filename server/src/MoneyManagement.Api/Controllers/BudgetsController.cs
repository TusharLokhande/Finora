using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyManagement.Api.Common;
using MoneyManagement.Application.Common.Interfaces;
using MoneyManagement.Application.Features.Budgets.Interfaces;
using MoneyManagement.Application.Features.Budgets.Requests;

namespace MoneyManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/budgets")]
public class BudgetsController : ControllerBase
{
    private readonly IBudgetService _budgetService;
    private readonly ICurrentUserService _currentUser;

    public BudgetsController(IBudgetService budgetService, ICurrentUserService currentUser)
    {
        _budgetService = budgetService;
        _currentUser = currentUser;
    }

    private Guid UserId => _currentUser.UserId!.Value;

    [HttpGet]
    public async Task<IActionResult> GetMonth([FromQuery] DateOnly month, CancellationToken cancellationToken)
    {
        var result = await _budgetService.GetMonthAsync(UserId, month, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>Upsert: creates the category's budget for the month, or replaces its amount.</summary>
    [HttpPut]
    public async Task<IActionResult> Set([FromBody] SetBudgetRequest request, CancellationToken cancellationToken)
    {
        var result = await _budgetService.SetAsync(UserId, request, cancellationToken);
        return result.ToActionResult();
    }

    [HttpPost("copy-from-previous")]
    public async Task<IActionResult> CopyFromPrevious([FromQuery] DateOnly month, CancellationToken cancellationToken)
    {
        var result = await _budgetService.CopyFromPreviousAsync(UserId, month, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("has-any")]
    public async Task<IActionResult> HasAny([FromQuery] DateOnly month, CancellationToken cancellationToken)
    {
        var result = await _budgetService.HasAnyAsync(UserId, month, cancellationToken);
        return result.ToActionResult();
    }
}
