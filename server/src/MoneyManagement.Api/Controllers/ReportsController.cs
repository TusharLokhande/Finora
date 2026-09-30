using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MoneyManagement.Api.Common;
using MoneyManagement.Application.Common.Interfaces;
using MoneyManagement.Application.Features.Reports.Interfaces;
using MoneyManagement.Application.Features.Reports.Requests;

namespace MoneyManagement.Api.Controllers;

/// <summary>Every endpoint takes the same range: <c>?months=6</c>, or <c>?from=2026-01-01&amp;to=2026-03-31</c>.</summary>
[ApiController]
[Authorize]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly ICurrentUserService _currentUser;

    public ReportsController(IReportService reportService, ICurrentUserService currentUser)
    {
        _reportService = reportService;
        _currentUser = currentUser;
    }

    private Guid UserId => _currentUser.UserId!.Value;

    [HttpGet("summary")]
    public async Task<IActionResult> Summary([FromQuery] ReportRangeRequest range, CancellationToken cancellationToken)
    {
        var result = await _reportService.GetSummaryAsync(UserId, range, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("by-category")]
    public async Task<IActionResult> ByCategory([FromQuery] ReportRangeRequest range, [FromQuery] int top = 5, CancellationToken cancellationToken = default)
    {
        var result = await _reportService.GetByCategoryAsync(UserId, range, top, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("by-category/{categoryId:guid}/subcategories")]
    public async Task<IActionResult> Subcategories(Guid categoryId, [FromQuery] ReportRangeRequest range, CancellationToken cancellationToken)
    {
        var result = await _reportService.GetSubcategoriesAsync(UserId, categoryId, range, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("income-vs-expense")]
    public async Task<IActionResult> IncomeVsExpense([FromQuery] ReportRangeRequest range, CancellationToken cancellationToken)
    {
        var result = await _reportService.GetIncomeVsExpenseAsync(UserId, range, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("budget-vs-actual")]
    public async Task<IActionResult> BudgetVsActual([FromQuery] ReportRangeRequest range, CancellationToken cancellationToken)
    {
        var result = await _reportService.GetBudgetVsActualAsync(UserId, range, cancellationToken);
        return result.ToActionResult();
    }

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] ReportRangeRequest range, CancellationToken cancellationToken)
    {
        var stream = new MemoryStream();
        var result = await _reportService.ExportAsync(UserId, range, stream, cancellationToken);
        if (!result.IsSuccess)
            return result.ToActionResult();

        stream.Position = 0;
        return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"report-{result.Data.From:yyyy-MM-dd}-to-{result.Data.To:yyyy-MM-dd}.xlsx");
    }
}
