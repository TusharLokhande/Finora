using Microsoft.EntityFrameworkCore;
using MoneyManagement.Application.Features.Budgets.Interfaces;
using MoneyManagement.Domain.Entities;

namespace MoneyManagement.Infrastructure.Persistence.Repository;

public class BudgetRepository : Repository<Budget>, IBudgetRepository
{
    public BudgetRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Budget>> GetForMonthAsync(Guid userId, DateOnly month, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AsNoTracking()
            .Where(b => b.UserId == userId && b.Active && b.Month == month)
            .ToListAsync(cancellationToken);
    }

    public async Task<Budget?> GetForCategoryMonthAsync(Guid userId, Guid categoryId, DateOnly month, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(
            b => b.UserId == userId && b.CategoryId == categoryId && b.Month == month && b.Active,
            cancellationToken);
    }

    public async Task<bool> AnyForMonthAsync(Guid userId, DateOnly month, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(b => b.UserId == userId && b.Month == month && b.Active, cancellationToken);
    }
}
