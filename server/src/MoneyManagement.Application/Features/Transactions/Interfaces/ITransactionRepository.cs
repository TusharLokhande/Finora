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
}
