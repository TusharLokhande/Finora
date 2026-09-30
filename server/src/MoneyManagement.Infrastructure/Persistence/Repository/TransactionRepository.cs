using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using MoneyManagement.Application.Common.Dashboard;
using MoneyManagement.Application.Features.Transactions.Dto;
using MoneyManagement.Application.Features.Transactions.Interfaces;
using MoneyManagement.Application.Features.Transactions.Requests;
using MoneyManagement.Infrastructure.Common;
using MoneyManagement.Domain.Entities;
using MoneyManagement.Domain.Enums;

namespace MoneyManagement.Infrastructure.Persistence.Repository;

public class TransactionRepository : Repository<Transaction>, ITransactionRepository
{
    public TransactionRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Transaction?> GetByIdForUserAsync(Guid userId, Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId, cancellationToken);
    }

    public async Task<IReadOnlyList<Transaction>> GetByIdsForUserAsync(Guid userId, IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
    {
        return await _dbSet.Where(t => t.UserId == userId && ids.Contains(t.Id)).ToListAsync(cancellationToken);
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

    // Column-header filters from the data table. Amount is compared in major units, as typed in the UI.
    private static readonly Dictionary<string, Expression<Func<Transaction, string>>> StringFilterFields = new()
    {
        ["description"] = t => t.Description!,
    };

    private static readonly Dictionary<string, LambdaExpression> NumberFilterFields = new()
    {
        ["amount"] = (Expression<Func<Transaction, decimal>>)(t => t.Amount / 100m),
    };

    private static readonly Dictionary<string, LambdaExpression> DateFilterFields = new()
    {
        ["txnDate"] = (Expression<Func<Transaction, DateOnly>>)(t => t.TxnDate),
    };

    public async Task<PageResult<TransactionDto>> SearchAsync(Guid userId, PageRequest<TransactionFilterRequest> request, CancellationToken cancellationToken = default)
    {
        var query = ApplyFilter(_dbSet.AsNoTracking().Where(t => t.UserId == userId && t.Active), request.CustomFilter)
            .ApplyFilters(request.Filters ?? [], StringFilterFields, NumberFilterFields, DateFilterFields);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await Sort(query, request.Sorting)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(t => new TransactionDto
            {
                Id = t.Id,
                Type = t.Type,
                TxnDate = t.TxnDate,
                Amount = t.Amount,
                AccountId = t.AccountId,
                ToAccountId = t.ToAccountId,
                CategoryId = t.CategoryId,
                Description = t.Description,
                Notes = t.Notes,
                AccountName = t.Account!.Name,
                ToAccountName = t.ToAccount != null ? t.ToAccount.Name : null,
                CategoryName = t.Category != null ? t.Category.Name : null,
                ParentCategoryName = t.Category != null && t.Category.Parent != null ? t.Category.Parent.Name : null,
            })
            .ToListAsync(cancellationToken);

        return new PageResult<TransactionDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
        };
    }

    public async Task<IReadOnlyList<DescriptionSuggestionDto>> GetDescriptionSuggestionsAsync(Guid userId, string prefix, int limit, CancellationToken cancellationToken = default)
    {
        var normalized = prefix.Trim().ToLower();

        // ponytail: newest 200 prefix matches, de-duplicated in memory. Fine for a personal ledger;
        // switch to the description_hints table (docs/data-model.md 3.9) if it gets slow.
        var recent = await _dbSet.AsNoTracking()
            .Where(t => t.UserId == userId && t.Active && t.Description != null && t.Description.ToLower().StartsWith(normalized))
            .OrderByDescending(t => t.TxnDate)
            .ThenByDescending(t => t.CreatedAtUtc)
            .Take(200)
            .Select(t => new DescriptionSuggestionDto
            {
                Description = t.Description!,
                Type = t.Type,
                CategoryId = t.CategoryId,
                AccountId = t.AccountId,
            })
            .ToListAsync(cancellationToken);

        return recent.DistinctBy(s => s.Description.Trim().ToLower()).Take(limit).ToList();
    }

    public async Task<IReadOnlyList<MonthlyTotalDto>> GetMonthlyTotalsAsync(Guid userId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var rows = await _dbSet.AsNoTracking()
            .Where(t => t.UserId == userId && t.Active && t.Type != TransactionType.Transfer && t.TxnDate >= from && t.TxnDate <= to)
            .GroupBy(t => new { t.TxnDate.Year, t.TxnDate.Month })
            .Select(g => new
            {
                g.Key.Year,
                g.Key.Month,
                Income = g.Where(t => t.Type == TransactionType.Income).Sum(t => (long?)t.Amount) ?? 0,
                Expense = g.Where(t => t.Type == TransactionType.Expense).Sum(t => (long?)t.Amount) ?? 0,
            })
            .ToListAsync(cancellationToken);

        return rows
            .Select(r => new MonthlyTotalDto { Month = new DateOnly(r.Year, r.Month, 1), Income = r.Income, Expense = r.Expense })
            .OrderBy(r => r.Month)
            .ToList();
    }

    public async Task<IReadOnlyDictionary<Guid, long>> GetExpenseByCategoryAsync(Guid userId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .Where(t => t.UserId == userId && t.Active && t.Type == TransactionType.Expense && t.CategoryId != null
                && t.TxnDate >= from && t.TxnDate <= to)
            .GroupBy(t => t.CategoryId!.Value)
            .Select(g => new { CategoryId = g.Key, Spent = g.Sum(t => t.Amount) })
            .ToDictionaryAsync(r => r.CategoryId, r => r.Spent, cancellationToken);
    }

    private static IQueryable<Transaction> ApplyFilter(IQueryable<Transaction> query, TransactionFilterRequest? f)
    {
        if (f is null)
            return query;

        if (f.From is not null) query = query.Where(t => t.TxnDate >= f.From);
        if (f.To is not null) query = query.Where(t => t.TxnDate <= f.To);
        if (f.Type is not null) query = query.Where(t => t.Type == f.Type);
        if (f.MinAmount is not null) query = query.Where(t => t.Amount >= f.MinAmount);
        if (f.MaxAmount is not null) query = query.Where(t => t.Amount <= f.MaxAmount);

        if (f.AccountIds is { Count: > 0 } accountIds)
            query = query.Where(t => accountIds.Contains(t.AccountId) || (t.ToAccountId != null && accountIds.Contains(t.ToAccountId.Value)));

        // A category matches itself and its sub-categories.
        if (f.CategoryIds is { Count: > 0 } categoryIds)
            query = query.Where(t => t.CategoryId != null
                && (categoryIds.Contains(t.CategoryId.Value) || (t.Category!.ParentId != null && categoryIds.Contains(t.Category.ParentId.Value))));

        if (!string.IsNullOrWhiteSpace(f.Search))
        {
            // ToLower().Contains translates to a LIKE on Postgres and also runs on the in-memory test provider.
            var term = f.Search.Trim().ToLower();
            query = query.Where(t => (t.Description != null && t.Description.ToLower().Contains(term))
                || (t.Notes != null && t.Notes.ToLower().Contains(term)));
        }

        return query;
    }

    private static IOrderedQueryable<Transaction> Sort(IQueryable<Transaction> query, PageSorting? sorting)
    {
        var desc = sorting?.Field is null || string.Equals(sorting.Direction, "desc", StringComparison.OrdinalIgnoreCase);

        IOrderedQueryable<Transaction> By<TKey>(Expression<Func<Transaction, TKey>> key) =>
            desc ? query.OrderByDescending(key) : query.OrderBy(key);

        var ordered = sorting?.Field?.ToLower() switch
        {
            "amount" => By(t => t.Amount),
            "description" => By(t => t.Description),
            "type" => By(t => t.Type),
            "account" => By(t => t.Account!.Name),
            "category" => By(t => t.Category!.Name),
            _ => By(t => t.TxnDate),
        };

        // Stable paging: newest-entered first within the same key.
        return ordered.ThenByDescending(t => t.TxnDate).ThenByDescending(t => t.CreatedAtUtc).ThenBy(t => t.Id);
    }
}
