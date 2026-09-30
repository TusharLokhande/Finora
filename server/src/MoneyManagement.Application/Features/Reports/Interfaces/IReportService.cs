using MoneyManagement.Application.Common;
using MoneyManagement.Application.Features.Reports.Dto;
using MoneyManagement.Application.Features.Reports.Requests;
using MoneyManagement.Application.Features.Transactions.Dto;

namespace MoneyManagement.Application.Features.Reports.Interfaces;

/// <summary>Reports over a date range. Transfers are excluded throughout.</summary>
public interface IReportService
{
    Task<Result<ReportSummaryDto>> GetSummaryAsync(Guid userId, ReportRangeRequest range, CancellationToken cancellationToken = default);

    /// <summary>Expense categories rolled up to their parent; the first <paramref name="top"/> individually, the rest as Other.</summary>
    Task<Result<CategoryBreakdownDto>> GetByCategoryAsync(Guid userId, ReportRangeRequest range, int top, CancellationToken cancellationToken = default);

    /// <summary>One top-level category split into its sub-categories (plus spend logged on the parent itself). Shares are of the parent's total.</summary>
    Task<Result<CategoryBreakdownDto>> GetSubcategoriesAsync(Guid userId, Guid categoryId, ReportRangeRequest range, CancellationToken cancellationToken = default);

    /// <summary>One row per calendar month the range touches, oldest first, gaps filled with zeros.</summary>
    Task<Result<IReadOnlyList<MonthlyTotalDto>>> GetIncomeVsExpenseAsync(Guid userId, ReportRangeRequest range, CancellationToken cancellationToken = default);

    Task<Result<BudgetVsActualDto>> GetBudgetVsActualAsync(Guid userId, ReportRangeRequest range, CancellationToken cancellationToken = default);

    /// <summary>By-category and income-vs-expense as one workbook, a sheet each.</summary>
    Task<Result<DateRange>> ExportAsync(Guid userId, ReportRangeRequest range, Stream output, CancellationToken cancellationToken = default);
}
