using MoneyManagement.Application.Interfaces.Repository;
using MoneyManagement.Domain.Entities;

namespace MoneyManagement.Application.Features.Budgets.Interfaces;

public interface IBudgetRepository : IRepository<Budget>
{
    /// <summary>Active budgets for the month starting on <paramref name="month"/> (always the 1st).</summary>
    Task<IReadOnlyList<Budget>> GetForMonthAsync(Guid userId, DateOnly month, CancellationToken cancellationToken = default);

    /// <summary>The active budget for one category and month, tracked for update.</summary>
    Task<Budget?> GetForCategoryMonthAsync(Guid userId, Guid categoryId, DateOnly month, CancellationToken cancellationToken = default);

    /// <summary>Every budget row the user owns, inactive included (used by the data export).</summary>
    Task<IReadOnlyList<Budget>> GetAllForUserAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<bool> AnyForMonthAsync(Guid userId, DateOnly month, CancellationToken cancellationToken = default);
}
