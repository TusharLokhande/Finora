using MoneyManagement.Application.Common;
using MoneyManagement.Application.Features.Budgets.Dto;
using MoneyManagement.Application.Features.Budgets.Interfaces;
using MoneyManagement.Application.Features.Budgets.Requests;
using MoneyManagement.Application.Features.Categories.Interfaces;
using MoneyManagement.Application.Features.Transactions.Interfaces;
using MoneyManagement.Application.Interfaces.UnitOfWork;
using MoneyManagement.Domain.Entities;
using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Application.Features.Budgets.Services;

public class BudgetService : IBudgetService
{
    private readonly IBudgetRepository _budgets;
    private readonly ICategoryRepository _categories;
    private readonly ITransactionRepository _transactions;
    private readonly IUnitOfWork _unitOfWork;

    public BudgetService(
        IBudgetRepository budgets,
        ICategoryRepository categories,
        ITransactionRepository transactions,
        IUnitOfWork unitOfWork)
    {
        _budgets = budgets;
        _categories = categories;
        _transactions = transactions;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<BudgetMonthDto>> GetMonthAsync(Guid userId, DateOnly month, CancellationToken cancellationToken = default)
    {
        if (month.Day != 1)
            return InvalidMonth<BudgetMonthDto>();

        var categories = await _categories.GetAllForUserAsync(userId, cancellationToken);
        var budgets = (await _budgets.GetForMonthAsync(userId, month, cancellationToken)).ToDictionary(b => b.CategoryId);
        var spend = await _transactions.GetExpenseByCategoryAsync(userId, month, month.AddMonths(1).AddDays(-1), cancellationToken);

        var childrenOf = categories.Where(c => c.ParentId is not null).ToLookup(c => c.ParentId!.Value);
        long Own(Guid id) => spend.GetValueOrDefault(id);
        long Rolled(Category c) => Own(c.Id) + (c.ParentId is null ? childrenOf[c.Id].Sum(child => Own(child.Id)) : 0);

        BudgetLineDto Line(Category c, Category? parent)
        {
            var budgeted = budgets.TryGetValue(c.Id, out var b) ? b.Amount : (long?)null;
            var spent = Rolled(c);
            return new BudgetLineDto
            {
                CategoryId = c.Id,
                ParentId = c.ParentId,
                CategoryName = c.Name,
                ParentCategoryName = parent?.Name,
                Icon = c.Icon ?? parent?.Icon,
                Color = c.Color ?? parent?.Color,
                SortOrder = c.SortOrder,
                Active = c.Active && (parent?.Active ?? true),
                Budgeted = budgeted,
                Spent = spent,
                PercentUsed = budgeted is long amount ? BudgetMath.PercentUsed(spent, amount) : null,
                Status = budgeted is long a ? BudgetMath.Status(spent, a) : BudgetStatus.Normal,
            };
        }

        var lines = new List<BudgetLineDto>();
        var roots = categories
            .Where(c => c.ParentId is null && c.Type == CategoryType.Expense && (c.Active || budgets.ContainsKey(c.Id)))
            .OrderBy(c => c.SortOrder).ThenBy(c => c.Name);

        foreach (var root in roots)
        {
            lines.Add(Line(root, null));
            foreach (var child in childrenOf[root.Id].Where(c => budgets.ContainsKey(c.Id)).OrderBy(c => c.SortOrder).ThenBy(c => c.Name))
                lines.Add(Line(child, root));
        }

        // Each transaction counts once, even when a category and its sub-category are both budgeted.
        var covered = new HashSet<Guid>();
        foreach (var id in budgets.Keys)
        {
            covered.Add(id);
            foreach (var child in childrenOf[id])
                covered.Add(child.Id);
        }

        return Result<BudgetMonthDto>.Success(new BudgetMonthDto
        {
            Month = month,
            // ponytail: budgets are summed as-is, so budgeting both Food and Food › Dining counts both
            // amounts, per docs ("sum of all category budgets"). Revisit if sub-budgets should nest.
            TotalBudgeted = budgets.Values.Sum(b => b.Amount),
            TotalSpent = covered.Sum(Own),
            Lines = lines,
        });
    }

    public async Task<Result<BudgetMonthDto>> SetAsync(Guid userId, SetBudgetRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Month.Day != 1)
            return InvalidMonth<BudgetMonthDto>();
        if (request.Amount < 0)
            return Result<BudgetMonthDto>.Failure("Budget can't be negative.", ErrorStatus.ValidationError);

        var category = await _categories.GetByIdForUserAsync(userId, request.CategoryId, cancellationToken);
        if (category is null)
            return Result<BudgetMonthDto>.Failure("Category not found.", ErrorStatus.NotFound);

        if (!category.Active)
            return Result<BudgetMonthDto>.Failure("This category is archived.", ErrorStatus.ValidationError);

        var parent = category.ParentId is Guid parentId
            ? await _categories.GetByIdForUserAsync(userId, parentId, cancellationToken)
            : null;

        if (category.Type != CategoryType.Expense || (parent is not null && parent.Type != CategoryType.Expense))
            return Result<BudgetMonthDto>.Failure("Budgets can only be set on expense categories.", ErrorStatus.ValidationError);

        var budget = await _budgets.GetForCategoryMonthAsync(userId, category.Id, request.Month, cancellationToken);
        if (budget is null)
        {
            await _budgets.AddAsync(new Budget
            {
                UserId = userId,
                CategoryId = category.Id,
                Month = request.Month,
                Amount = request.Amount,
                CreatedBy = userId,
            }, cancellationToken);
        }
        else
        {
            budget.Amount = request.Amount;
            budget.UpdatedBy = userId;
            budget.UpdatedAtUtc = DateTime.UtcNow;
            _budgets.Update(budget);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var month = await GetMonthAsync(userId, request.Month, cancellationToken);
        month.Message = "Budget saved.";
        return month;
    }

    public async Task<Result<int>> CopyFromPreviousAsync(Guid userId, DateOnly month, CancellationToken cancellationToken = default)
    {
        if (month.Day != 1)
            return InvalidMonth<int>();

        // Never merge into a month the user has already started.
        if (await _budgets.AnyForMonthAsync(userId, month, cancellationToken))
            return Result<int>.Failure("This month already has budgets.", ErrorStatus.Duplicate);

        var previous = await _budgets.GetForMonthAsync(userId, month.AddMonths(-1), cancellationToken);

        await _budgets.AddRangeAsync(previous.Select(b => new Budget
        {
            UserId = userId,
            CategoryId = b.CategoryId,
            Month = month,
            Amount = b.Amount,
            CreatedBy = userId,
        }), cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<int>.Success(previous.Count, previous.Count == 0 ? "Last month had no budgets to copy." : $"Copied {previous.Count} budget(s).");
    }

    public async Task<Result<bool>> HasAnyAsync(Guid userId, DateOnly month, CancellationToken cancellationToken = default)
    {
        if (month.Day != 1)
            return InvalidMonth<bool>();

        return Result<bool>.Success(await _budgets.AnyForMonthAsync(userId, month, cancellationToken));
    }

    private static Result<T> InvalidMonth<T>()
        => Result<T>.Failure("Month must be the first day of a month.", ErrorStatus.ValidationError);
}
