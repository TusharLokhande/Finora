using MoneyManagement.Application.Common;
using MoneyManagement.Application.Features.Budgets.Dto;
using MoneyManagement.Application.Features.Budgets.Requests;

namespace MoneyManagement.Application.Features.Budgets.Interfaces;

/// <summary>Monthly category budgets. Every <c>month</c> is the first day of a month.</summary>
public interface IBudgetService
{
    /// <summary>
    /// Budget vs actual: every active top-level expense category (budgeted or not), plus sub-categories
    /// and archived categories that have a budget this month. The single source for budget numbers.
    /// </summary>
    Task<Result<BudgetMonthDto>> GetMonthAsync(Guid userId, DateOnly month, CancellationToken cancellationToken = default);

    /// <summary>Creates or replaces one category's budget. Returns the refreshed month.</summary>
    Task<Result<BudgetMonthDto>> SetAsync(Guid userId, SetBudgetRequest request, CancellationToken cancellationToken = default);

    /// <summary>Copies the previous month's rows into <paramref name="month"/>. Refused if it already has any. Returns the count.</summary>
    Task<Result<int>> CopyFromPreviousAsync(Guid userId, DateOnly month, CancellationToken cancellationToken = default);

    Task<Result<bool>> HasAnyAsync(Guid userId, DateOnly month, CancellationToken cancellationToken = default);
}
