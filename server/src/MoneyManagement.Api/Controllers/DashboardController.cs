using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyManagement.Api.Common;
using MoneyManagement.Application.Common.Interfaces;
using MoneyManagement.Application.Features.Dashboard.Interfaces;

namespace MoneyManagement.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly ICurrentUserService _currentUser;

    public DashboardController(IDashboardService dashboardService, ICurrentUserService currentUser)
    {
        _dashboardService = dashboardService;
        _currentUser = currentUser;
    }

    private Guid UserId => _currentUser.UserId!.Value;

    [HttpGet("summary")]
    public async Task<IActionResult> Summary(CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetSummaryAsync(UserId, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("budgets-at-risk")]
    public async Task<IActionResult> BudgetsAtRisk(CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetBudgetsAtRiskAsync(UserId, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("top-categories")]
    public async Task<IActionResult> TopCategories([FromQuery] int limit = 3, CancellationToken cancellationToken = default)
    {
        var result = await _dashboardService.GetTopCategoriesAsync(UserId, limit, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("upcoming-dues")]
    public async Task<IActionResult> UpcomingDues(CancellationToken cancellationToken)
    {
        var result = await _dashboardService.GetUpcomingDuesAsync(UserId, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("income-expense")]
    public async Task<IActionResult> IncomeExpense([FromQuery] int months = 6, CancellationToken cancellationToken = default)
    {
        var result = await _dashboardService.GetIncomeExpenseAsync(UserId, months, cancellationToken);
        return result.ToActionResult();
    }
}
