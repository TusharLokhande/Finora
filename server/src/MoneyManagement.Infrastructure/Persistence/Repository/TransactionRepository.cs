using Microsoft.EntityFrameworkCore;
using MoneyManagement.Application.Features.Transactions.Interfaces;
using MoneyManagement.Domain.Entities;
using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Infrastructure.Persistence.Repository;

public class TransactionRepository : Repository<Transaction>, ITransactionRepository
{
    public TransactionRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyDictionary<Guid, long>> GetBalanceDeltasAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var outflows = await _context.Transactions
            .Where(t => t.UserId == userId && t.Active)
            .GroupBy(t => t.AccountId)
            .Select(g => new
            {
                AccountId = g.Key,
                Income = g.Where(t => t.Type == TransactionType.Income).Sum(t => (long?)t.Amount) ?? 0,
                Expense = g.Where(t => t.Type == TransactionType.Expense).Sum(t => (long?)t.Amount) ?? 0,
                TransferOut = g.Where(t => t.Type == TransactionType.Transfer).Sum(t => (long?)t.Amount) ?? 0,
            })
            .ToListAsync(cancellationToken);

        var inflows = await _context.Transactions
            .Where(t => t.UserId == userId && t.Active && t.ToAccountId != null)
            .GroupBy(t => t.ToAccountId!.Value)
            .Select(g => new { AccountId = g.Key, TransferIn = g.Sum(t => t.Amount) })
            .ToListAsync(cancellationToken);

        var deltas = new Dictionary<Guid, long>();

        foreach (var o in outflows)
            deltas[o.AccountId] = deltas.GetValueOrDefault(o.AccountId) + o.Income - o.Expense - o.TransferOut;

        foreach (var i in inflows)
            deltas[i.AccountId] = deltas.GetValueOrDefault(i.AccountId) + i.TransferIn;

        return deltas;
    }
}
