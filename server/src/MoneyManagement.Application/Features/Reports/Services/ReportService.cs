using MoneyManagement.Application.Common;
using MoneyManagement.Application.Common.Export;
using MoneyManagement.Application.Common.Interfaces;
using MoneyManagement.Application.Common.Services;
using MoneyManagement.Application.Features.Budgets;
using MoneyManagement.Application.Features.Budgets.Dto;
using MoneyManagement.Application.Features.Budgets.Interfaces;
using MoneyManagement.Application.Features.Categories.Interfaces;
using MoneyManagement.Application.Features.Reports.Dto;
using MoneyManagement.Application.Features.Reports.Interfaces;
using MoneyManagement.Application.Features.Reports.Requests;
using MoneyManagement.Application.Features.Transactions.Dto;
using MoneyManagement.Application.Features.Transactions.Interfaces;
using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Application.Features.Reports.Services;

public class ReportService : IReportService
{
    private readonly ITransactionRepository _transactions;
    private readonly ICategoryRepository _categories;
    private readonly IBudgetService _budgets;
    private readonly UserClock _clock;
    private readonly IExcelExportWriter _excel;

    public ReportService(
        ITransactionRepository transactions,
        ICategoryRepository categories,
        IBudgetService budgets,
        UserClock clock,
        IExcelExportWriter excel)
    {
        _transactions = transactions;
        _categories = categories;
        _budgets = budgets;
        _clock = clock;
        _excel = excel;
    }

    private const int MaxTop = 20;

    public async Task<Result<ReportSummaryDto>> GetSummaryAsync(Guid userId, ReportRangeRequest request, CancellationToken cancellationToken = default)
    {
        var (range, error) = await ResolveAsync(userId, request, cancellationToken);
        if (error is not null)
            return Result<ReportSummaryDto>.Failure(error, ErrorStatus.ValidationError);

        var prior = DateRangeMath.PriorPeriod(range, request.From is null ? request.Months ?? 6 : null);
        var (spent, income) = await TotalsAsync(userId, range, cancellationToken);
        var (priorSpent, priorIncome) = await TotalsAsync(userId, prior, cancellationToken);

        return Result<ReportSummaryDto>.Success(new ReportSummaryDto
        {
            From = range.From,
            To = range.To,
            PriorFrom = prior.From,
            PriorTo = prior.To,
            Spent = spent,
            Income = income,
            NetSavings = income - spent,
            PriorSpent = priorSpent,
            PriorIncome = priorIncome,
            PriorNetSavings = priorIncome - priorSpent,
            SpentChangePercent = PercentMath.ChangePercent(spent, priorSpent),
            IncomeChangePercent = PercentMath.ChangePercent(income, priorIncome),
            NetSavingsChangePercent = PercentMath.ChangePercent(income - spent, priorIncome - priorSpent),
            SavingsRatePercent = PercentMath.Percent(income - spent, income),
        });
    }

    public async Task<Result<CategoryBreakdownDto>> GetByCategoryAsync(Guid userId, ReportRangeRequest request, int top, CancellationToken cancellationToken = default)
    {
        var (range, error) = await ResolveAsync(userId, request, cancellationToken);
        if (error is not null)
            return Result<CategoryBreakdownDto>.Failure(error, ErrorStatus.ValidationError);

        var spend = await _transactions.GetExpenseByCategoryAsync(userId, range.From, range.To, cancellationToken);
        var byId = (await _categories.GetAllForUserAsync(userId, cancellationToken)).ToDictionary(c => c.Id);
        var total = spend.Values.Sum();

        // Roll each sub-category up into its parent (same rollup as docs/data-model.md §6).
        Guid RootOf(Guid id) => byId.TryGetValue(id, out var c) ? c.ParentId ?? c.Id : id;
        var ranked = spend
            .GroupBy(kv => RootOf(kv.Key))
            .Select(g => new
            {
                CategoryId = g.Key,
                Spent = g.Sum(kv => kv.Value),
                HasSubcategories = g.Any(kv => kv.Key != g.Key),
            })
            .OrderByDescending(x => x.Spent)
            .ThenBy(x => byId.GetValueOrDefault(x.CategoryId)?.Name)
            .ToList();

        var shown = ranked.Take(Math.Clamp(top, 1, MaxTop)).ToList();
        var rest = ranked.Skip(shown.Count).ToList();

        return Result<CategoryBreakdownDto>.Success(new CategoryBreakdownDto
        {
            From = range.From,
            To = range.To,
            Total = total,
            Categories = shown.Select(x =>
            {
                var category = byId.GetValueOrDefault(x.CategoryId);
                return new CategorySpendDto
                {
                    CategoryId = x.CategoryId,
                    Name = category?.Name ?? "Unknown",
                    Icon = category?.Icon,
                    Color = category?.Color,
                    Spent = x.Spent,
                    SharePercent = PercentMath.Percent(x.Spent, total) ?? 0,
                    HasSubcategories = x.HasSubcategories,
                };
            }).ToList(),
            Other = rest.Count == 0 ? null : new OtherSpendDto
            {
                CategoryCount = rest.Count,
                Spent = rest.Sum(x => x.Spent),
                SharePercent = PercentMath.Percent(rest.Sum(x => x.Spent), total) ?? 0,
            },
        });
    }

    public async Task<Result<CategoryBreakdownDto>> GetSubcategoriesAsync(Guid userId, Guid categoryId, ReportRangeRequest request, CancellationToken cancellationToken = default)
    {
        var (range, error) = await ResolveAsync(userId, request, cancellationToken);
        if (error is not null)
            return Result<CategoryBreakdownDto>.Failure(error, ErrorStatus.ValidationError);

        var parent = await _categories.GetByIdForUserAsync(userId, categoryId, cancellationToken);
        if (parent is null || parent.ParentId is not null || parent.Type != CategoryType.Expense)
            return Result<CategoryBreakdownDto>.Failure("Expense category not found.", ErrorStatus.NotFound);

        var spend = await _transactions.GetExpenseByCategoryAsync(userId, range.From, range.To, cancellationToken);
        var children = (await _categories.GetAllForUserAsync(userId, cancellationToken)).Where(c => c.ParentId == parent.Id).ToList();

        var rows = children
            .Select(c => (Id: c.Id, Name: c.Name, Icon: c.Icon, Color: c.Color, Spent: spend.GetValueOrDefault(c.Id)))
            .Append((Id: parent.Id, Name: $"{parent.Name} (no sub-category)", Icon: parent.Icon, Color: parent.Color, Spent: spend.GetValueOrDefault(parent.Id)))
            .Where(r => r.Spent > 0)
            .OrderByDescending(r => r.Spent)
            .ThenBy(r => r.Name)
            .ToList();
        var total = rows.Sum(r => r.Spent);

        return Result<CategoryBreakdownDto>.Success(new CategoryBreakdownDto
        {
            From = range.From,
            To = range.To,
            Total = total,
            Categories = rows.Select(r => new CategorySpendDto
            {
                CategoryId = r.Id,
                Name = r.Name,
                Icon = r.Icon ?? parent.Icon,
                Color = r.Color ?? parent.Color,
                Spent = r.Spent,
                SharePercent = PercentMath.Percent(r.Spent, total) ?? 0,
            }).ToList(),
        });
    }

    public async Task<Result<IReadOnlyList<MonthlyTotalDto>>> GetIncomeVsExpenseAsync(Guid userId, ReportRangeRequest request, CancellationToken cancellationToken = default)
    {
        var (range, error) = await ResolveAsync(userId, request, cancellationToken);
        if (error is not null)
            return Result<IReadOnlyList<MonthlyTotalDto>>.Failure(error, ErrorStatus.ValidationError);

        return Result<IReadOnlyList<MonthlyTotalDto>>.Success(await MonthlySeriesAsync(userId, range, cancellationToken));
    }

    public async Task<Result<BudgetVsActualDto>> GetBudgetVsActualAsync(Guid userId, ReportRangeRequest request, CancellationToken cancellationToken = default)
    {
        var (range, error) = await ResolveAsync(userId, request, cancellationToken);
        if (error is not null)
            return Result<BudgetVsActualDto>.Failure(error, ErrorStatus.ValidationError);

        var sums = new Dictionary<Guid, (BudgetLineDto Latest, long Budgeted, long Spent)>();
        var order = new List<Guid>();
        int monthsWithBudgets = 0;
        long totalBudgeted = 0, totalSpent = 0;

        // Budgets are monthly, so every month the range touches counts in full — exactly what the Budgets
        // page shows for that month. ponytail: one budget query per month (<= 24), fine at this scale.
        foreach (var month in DateRangeMath.MonthsIn(range))
        {
            var result = await _budgets.GetMonthAsync(userId, month, cancellationToken);
            if (!result.IsSuccess)
                return Result<BudgetVsActualDto>.Failure(result.Message, result.ErrorStatus);

            var budgeted = result.Data!.Lines.Where(l => l.Budgeted is not null).ToList();
            if (budgeted.Count == 0)
                continue;

            monthsWithBudgets++;
            totalBudgeted += result.Data.TotalBudgeted;
            totalSpent += result.Data.TotalSpent;

            // A category's spend is only summed for months where it has its own budget row.
            foreach (var line in budgeted)
            {
                if (!sums.TryGetValue(line.CategoryId, out var sum))
                    order.Add(line.CategoryId);
                sums[line.CategoryId] = (line, sum.Budgeted + line.Budgeted!.Value, sum.Spent + line.Spent);
            }
        }

        var lines = order
            .Select(id =>
            {
                var (latest, budgeted, spent) = sums[id];
                return new BudgetLineDto
                {
                    CategoryId = id,
                    ParentId = latest.ParentId,
                    CategoryName = latest.CategoryName,
                    ParentCategoryName = latest.ParentCategoryName,
                    Icon = latest.Icon,
                    Color = latest.Color,
                    SortOrder = latest.SortOrder,
                    Active = latest.Active,
                    Budgeted = budgeted,
                    Spent = spent,
                    PercentUsed = BudgetMath.PercentUsed(spent, budgeted),
                    Status = BudgetMath.Status(spent, budgeted),
                };
            })
            .OrderByDescending(l => l.PercentUsed ?? (l.Status == BudgetStatus.Over ? decimal.MaxValue : 0))
            .ThenBy(l => l.CategoryName)
            .ToList();

        return Result<BudgetVsActualDto>.Success(new BudgetVsActualDto
        {
            From = range.From,
            To = range.To,
            MonthsWithBudgets = monthsWithBudgets,
            TotalBudgeted = totalBudgeted,
            TotalSpent = totalSpent,
            Lines = lines,
        });
    }

    public async Task<Result<DateRange>> ExportAsync(Guid userId, ReportRangeRequest request, Stream output, CancellationToken cancellationToken = default)
    {
        var (range, error) = await ResolveAsync(userId, request, cancellationToken);
        if (error is not null)
            return Result<DateRange>.Failure(error, ErrorStatus.ValidationError);

        // Every category individually in the export, rather than the on-screen top N + Other.
        var byCategory = (await GetByCategoryAsync(userId, request, MaxTop, cancellationToken)).Data!;
        var otherRow = byCategory.Other is { } o ? new object?[] { $"Other ({o.CategoryCount} categories)", o.Spent, o.SharePercent } : null;
        var months = await MonthlySeriesAsync(userId, range, cancellationToken);

        await _excel.WriteAsync(
        [
            new ExcelSheet("By category",
                [new("Category"), new("Spent", ExcelFormat.Money), new("Share %", ExcelFormat.Percent)],
                byCategory.Categories.Select(c => new object?[] { c.Name, c.Spent, c.SharePercent })
                    .Concat(otherRow is null ? [] : [otherRow])
                    .Append(["Total", byCategory.Total, 100m])
                    .ToList()),
            new ExcelSheet("Income vs expense",
                [new("Month", ExcelFormat.Date), new("Income", ExcelFormat.Money), new("Expense", ExcelFormat.Money), new("Net", ExcelFormat.Money)],
                months.Select(m => new object?[] { m.Month, m.Income, m.Expense, m.Net }).ToList()),
        ], output, cancellationToken);

        return Result<DateRange>.Success(range);
    }

    private async Task<(DateRange Range, string? Error)> ResolveAsync(Guid userId, ReportRangeRequest request, CancellationToken cancellationToken)
        => DateRangeMath.Resolve(request.Months, request.From, request.To, await _clock.TodayAsync(userId, cancellationToken));

    private async Task<(long Spent, long Income)> TotalsAsync(Guid userId, DateRange range, CancellationToken cancellationToken)
    {
        var months = await _transactions.GetMonthlyTotalsAsync(userId, range.From, range.To, cancellationToken);
        return (months.Sum(m => m.Expense), months.Sum(m => m.Income));
    }

    private async Task<IReadOnlyList<MonthlyTotalDto>> MonthlySeriesAsync(Guid userId, DateRange range, CancellationToken cancellationToken)
    {
        var byMonth = (await _transactions.GetMonthlyTotalsAsync(userId, range.From, range.To, cancellationToken)).ToDictionary(t => t.Month);
        return DateRangeMath.MonthsIn(range)
            .Select(m => byMonth.GetValueOrDefault(m) ?? new MonthlyTotalDto { Month = m })
            .ToList();
    }
}
