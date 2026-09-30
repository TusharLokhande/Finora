using MoneyManagement.Application.Common.Dashboard;
using MoneyManagement.Application.Features.Transactions.Dto;
using MoneyManagement.Application.Features.Transactions.Requests;
using MoneyManagement.Application.Interfaces.Repository;
using MoneyManagement.Domain.Entities;

namespace MoneyManagement.Application.Features.Transactions.Interfaces;

public interface ITransactionRepository : IRepository<Transaction>
{
    /// <summary>
    /// Net balance delta per account from all active transactions (excludes opening balance).
    /// Matches the formula in docs/data-model.md section 4.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, long>> GetBalanceDeltasAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Includes soft-deleted transactions, so restore can find them.</summary>
    Task<Transaction?> GetByIdForUserAsync(Guid userId, Guid id, CancellationToken cancellationToken = default);

    /// <summary>Includes soft-deleted transactions. Ids that don't belong to the user are simply absent.</summary>
    Task<IReadOnlyList<Transaction>> GetByIdsForUserAsync(Guid userId, IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);

    /// <summary>Active transactions for the user, filtered, sorted and paged. Default order is newest date first.</summary>
    Task<PageResult<TransactionDto>> SearchAsync(Guid userId, PageRequest<TransactionFilterRequest> request, CancellationToken cancellationToken = default);

    /// <summary>Most recent distinct descriptions starting with <paramref name="prefix"/>, newest first.</summary>
    Task<IReadOnlyList<DescriptionSuggestionDto>> GetDescriptionSuggestionsAsync(Guid userId, string prefix, int limit, CancellationToken cancellationToken = default);

    /// <summary>Income/expense per calendar month in [from, to]. Months with no activity are omitted.</summary>
    Task<IReadOnlyList<MonthlyTotalDto>> GetMonthlyTotalsAsync(Guid userId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);

    /// <summary>Expense total per category id (not rolled up to parents) in [from, to].</summary>
    Task<IReadOnlyDictionary<Guid, long>> GetExpenseByCategoryAsync(Guid userId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
}
