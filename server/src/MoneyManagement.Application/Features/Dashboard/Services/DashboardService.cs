using MoneyManagement.Application.Common;
using MoneyManagement.Application.Features.Accounts.Interfaces;
using MoneyManagement.Application.Features.Budgets.Dto;
using MoneyManagement.Application.Features.Budgets.Interfaces;
using MoneyManagement.Application.Common.Services;
using MoneyManagement.Application.Features.Dashboard.Dto;
using MoneyManagement.Application.Features.Dashboard.Interfaces;
using MoneyManagement.Application.Features.Reports.Dto;
using MoneyManagement.Application.Features.Reports.Interfaces;
using MoneyManagement.Application.Features.Reports.Requests;
using MoneyManagement.Application.Features.Transactions.Dto;
using MoneyManagement.Application.Features.Transactions.Interfaces;
using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Application.Features.Dashboard.Services;

public class DashboardService : IDashboardService
{
    private readonly ITransactionRepository _transactions;
    private readonly IBudgetService _budgetService;
    private readonly IReportService _reports;
    private readonly IAccountService _accountService;
    private readonly UserClock _clock;

    public DashboardService(
        ITransactionRepository transactions,
        IBudgetService budgetService,
        IReportService reports,
        IAccountService accountService,
        UserClock clock)
    {
        _transactions = transactions;
        _budgetService = budgetService;
        _reports = reports;
        _accountService = accountService;
        _clock = clock;
    }

    public async Task<Result<DashboardSummaryDto>> GetSummaryAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var thisMonth = StartOfMonth(await TodayAsync(userId, cancellationToken));
        var lastMonth = thisMonth.AddMonths(-1);

        var totals = await _transactions.GetMonthlyTotalsAsync(userId, lastMonth, EndOfMonth(thisMonth), cancellationToken);

        var current = await BuildMonthSummaryAsync(userId, thisMonth, totals, cancellationToken);
        var previous = await BuildMonthSummaryAsync(userId, lastMonth, totals, cancellationToken);

        return Result<DashboardSummaryDto>.Success(new DashboardSummaryDto
        {
            Current = current,
            Previous = previous,
            SpentChangePercent = PercentMath.ChangePercent(current.Spent, previous.Spent),
            IncomeChangePercent = PercentMath.ChangePercent(current.Income, previous.Income),
            NetSavingsChangePercent = PercentMath.ChangePercent(current.NetSavings, previous.NetSavings),
            SavingsRatePercent = PercentMath.Percent(current.NetSavings, current.Income),
        });
    }

    public async Task<Result<IReadOnlyList<BudgetLineDto>>> GetBudgetsAtRiskAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var thisMonth = StartOfMonth(await TodayAsync(userId, cancellationToken));
        var month = await _budgetService.GetMonthAsync(userId, thisMonth, cancellationToken);
        if (!month.IsSuccess)
            return Result<IReadOnlyList<BudgetLineDto>>.Failure(month.Message, month.ErrorStatus);

        // Same status the Budgets page shows; a zero budget with spend (no percentage) sorts first.
        var atRisk = month.Data!.Lines
            .Where(l => l.Budgeted is not null && l.Status != BudgetStatus.Normal)
            .OrderByDescending(l => l.PercentUsed ?? decimal.MaxValue)
            .ThenBy(l => l.CategoryName)
            .ToList();

        return Result<IReadOnlyList<BudgetLineDto>>.Success(atRisk);
    }

    /// <summary>The Reports by-category breakdown for this month, without the Other bucket.</summary>
    public async Task<Result<IReadOnlyList<CategorySpendDto>>> GetTopCategoriesAsync(Guid userId, int limit, CancellationToken cancellationToken = default)
    {
        var breakdown = await _reports.GetByCategoryAsync(userId, new ReportRangeRequest { Months = 1 }, limit, cancellationToken);
        return breakdown.IsSuccess
            ? Result<IReadOnlyList<CategorySpendDto>>.Success(breakdown.Data!.Categories)
            : Result<IReadOnlyList<CategorySpendDto>>.Failure(breakdown.Message, breakdown.ErrorStatus);
    }

    public async Task<Result<IReadOnlyList<UpcomingDueDto>>> GetUpcomingDuesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var today = await TodayAsync(userId, cancellationToken);
        var accounts = await _accountService.GetAllAsync(userId, cancellationToken);
        if (!accounts.IsSuccess)
            return Result<IReadOnlyList<UpcomingDueDto>>.Failure(accounts.Message, accounts.ErrorStatus);

        var dues = accounts.Data!
            .Where(a => a.Type == AccountType.CreditCard && a.Active && a.NextDueDate is not null)
            .OrderBy(a => a.NextDueDate)
            .ThenBy(a => a.SortOrder)
            .Select(a => new UpcomingDueDto
            {
                AccountId = a.Id,
                Name = a.Name,
                Color = a.Color,
                Outstanding = a.Outstanding ?? 0,
                CreditLimit = a.CreditLimit,
                DueDate = a.NextDueDate!.Value,
                DaysUntilDue = a.NextDueDate!.Value.DayNumber - today.DayNumber,
            })
            .ToList();

        return Result<IReadOnlyList<UpcomingDueDto>>.Success(dues);
    }

    /// <summary>The Reports income-vs-expense series with a fixed "last N months" range.</summary>
    public Task<Result<IReadOnlyList<MonthlyTotalDto>>> GetIncomeExpenseAsync(Guid userId, int months, CancellationToken cancellationToken = default)
        => _reports.GetIncomeVsExpenseAsync(userId, new ReportRangeRequest { Months = Math.Clamp(months, 1, DateRangeMath.MaxMonths) }, cancellationToken);

    private async Task<MonthSummaryDto> BuildMonthSummaryAsync(
        Guid userId, DateOnly month, IReadOnlyList<MonthlyTotalDto> totals, CancellationToken cancellationToken)
    {
        var total = totals.FirstOrDefault(t => t.Month == month) ?? new MonthlyTotalDto { Month = month };
        var budgets = (await _budgetService.GetMonthAsync(userId, month, cancellationToken)).Data!;

        return new MonthSummaryDto
        {
            Month = month,
            Spent = total.Expense,
            Income = total.Income,
            NetSavings = total.Net,
            Budgeted = budgets.TotalBudgeted,
            BudgetRemaining = budgets.Remaining,
        };
    }

    private Task<DateOnly> TodayAsync(Guid userId, CancellationToken cancellationToken) => _clock.TodayAsync(userId, cancellationToken);

    private static DateOnly StartOfMonth(DateOnly date) => DateRangeMath.StartOfMonth(date);

    private static DateOnly EndOfMonth(DateOnly date) => DateRangeMath.EndOfMonth(date);
}
