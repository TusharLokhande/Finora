using MoneyManagement.Application.Common;
using MoneyManagement.Application.Features.Budgets.Dto;
using MoneyManagement.Application.Features.Dashboard.Dto;
using MoneyManagement.Application.Features.Reports.Dto;
using MoneyManagement.Application.Features.Transactions.Dto;

namespace MoneyManagement.Application.Features.Dashboard.Interfaces;

/// <summary>Dashboard widgets. "This month" is the calendar month in the user's timezone.</summary>
public interface IDashboardService
{
    Task<Result<DashboardSummaryDto>> GetSummaryAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>This month's budgets at or above 80% used, most used first.</summary>
    Task<Result<IReadOnlyList<BudgetLineDto>>> GetBudgetsAtRiskAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Result<IReadOnlyList<CategorySpendDto>>> GetTopCategoriesAsync(Guid userId, int limit, CancellationToken cancellationToken = default);

    /// <summary>Active credit cards, nearest due date first.</summary>
    Task<Result<IReadOnlyList<UpcomingDueDto>>> GetUpcomingDuesAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Income vs expense for the last <paramref name="months"/> calendar months (including this one), oldest first, gaps filled with zeros.</summary>
    Task<Result<IReadOnlyList<MonthlyTotalDto>>> GetIncomeExpenseAsync(Guid userId, int months, CancellationToken cancellationToken = default);
}
